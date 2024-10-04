using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration;

/// <summary>
/// Represents a map of data transaction rules and transfer definitions for database schema migration.
/// </summary>
/// <remarks>
///     <para>
///     This class is used to manage and apply rules for creating, updating, and deleting database schema elements
///     during a migration process, and contains transfer definitions for moving data between tables.
///     </para>
///     <para>
///     The class also provides methods for merging multiple <see cref="MigrationPhase"/> instances into a single map.
///     </para>
/// </remarks>
public class DataTransactionMap : IDictionary<string, TransactionRuleDictionary>, ICollection
{
    /// <summary>
    /// The variable used for column casting in SQL scripts.
    /// </summary>
    private const string CastVariable = "{column}";

    private static readonly KeyValuePair<string, DataTransactionRule> DefaultRecord = default;

    private readonly List<string> _orderList = [];
    private readonly Dictionary<string, TransactionRuleDictionary> _map = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTransactionMap"/> class.
    /// </summary>
    public DataTransactionMap()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTransactionMap"/> class.
    /// </summary>
    /// <param name="phases">The <see cref="MigrationPhase"/> collection to be merged.</param>
    /// <param name="currentVersion">The version of the current DB schema.</param>
    public DataTransactionMap(IEnumerable<MigrationPhase> phases, Version? currentVersion = null)
    {
        ValidateRoadmap(phases);
        var latest = phases.Max(p => p.Version);

        foreach (var phase in phases)
        {
            Merge(phase, currentVersion);

            var transfer = phase.Guidelines.Transfer;
            if (transfer.Count == 0 || currentVersion >= phase.Version || phase.Version >= latest)
                continue;

            foreach (var (table, definition) in transfer)
            {
                TransferDefinitions[table] = definition;
            }
        }
    }

    /// <summary>
    /// Gets or sets the lists of SQL scripts to be executed.
    /// </summary>
    public Dictionary<string, DataTransferDefinition> TransferDefinitions { get; set; } = [];

    /// <summary>
    /// Gets the number of elements contained in the <see cref="DataTransactionMap"/>.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Gets a value indicating whether the <see cref="DataTransactionMap"/> is read-only.
    /// </summary>
    public bool IsReadOnly => true;

    /// <summary>
    /// Gets a value indicating whether access to the <see cref="DataTransactionMap"/> is synchronized (thread safe).
    /// </summary>
    public bool IsSynchronized => false;

    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="DataTransactionMap"/>.
    /// </summary>
    public object SyncRoot => this;

    /// <summary>
    /// Gets a collection containing the keys of the <see cref="DataTransactionMap"/>.
    /// </summary>
    public ICollection<string> Keys => _orderList;

    /// <summary>
    /// Gets a collection containing the values in the <see cref="DataTransactionMap"/>.
    /// </summary>
    public ICollection<TransactionRuleDictionary> Values => _map.Values;

    /// <summary>
    /// Gets or sets the <see cref="TransactionRuleDictionary"/> with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <returns>The <see cref="TransactionRuleDictionary"/> with the specified key.</returns>
    public TransactionRuleDictionary this[string key]
    {
        get => _map[key];
        set
        {
            _map[key] = value;
            _orderList.Add(key);
        }
    }

    private static void ValidateRoadmap(IEnumerable<MigrationPhase> roadmap)
    {
        if (!roadmap.Any())
        {
            throw new MigrationException(Resources.EmptyRoadmap);
        }

        var duplicate = roadmap
            .GroupBy(p => p.Version)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .FirstOrDefault();

        if (duplicate is not null)
        {
            throw new MigrationException("The roadmap contains duplicate versions: " + duplicate);
        }
    }

    /// <summary>
    /// Merges the specified <see cref="MigrationPhase"/> into the <see cref="DataTransactionMap"/>.
    /// </summary>
    /// <param name="phase">The <see cref="MigrationPhase"/> to merge.</param>
    /// <param name="currentVersion">The version of the current DB schema.</param>
    public void Merge(MigrationPhase phase, Version? currentVersion = null)
    {
        var performed = currentVersion != null && currentVersion >= phase.Version;

        MergeCreate(phase.Guidelines.Create, performed);
        MergeUpdate(phase.Guidelines.Update, performed);
        MergeDelete(phase.Guidelines.Delete);
    }

    /// <summary>
    /// Merges the "create" guidelines into the <see cref="DataTransactionMap"/>.
    /// </summary>
    /// <param name="create">The "create" guidelines.</param>
    /// <param name="alreadyPerformed">A value indicating whether the guidelines have already been performed.</param>
    private void MergeCreate(Dictionary<string, ColumnsDefinition> create, bool alreadyPerformed)
    {
        if (create is null || create.Count == 0)
            return;

        foreach (var (table, collection) in create)
        {
            if (!_map.TryGetValue(table, out var rules))
            {
                _map[table] = new TransactionRuleDictionary { Condition = collection.Condition };

                if (!string.IsNullOrEmpty(collection.Before))
                {
                    var index = _orderList.IndexOf(collection.Before);
                    if (index == -1)
                    {
                        throw new MigrationException($"Cannot execute table ({table}) before ({collection.Before}) because it doesn't exist.");
                    }

                    _orderList.Insert(index, table);
                }
                else
                {
                    _orderList.Add(table);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(collection.Condition))
                {
                    rules.Condition = collection.Condition;
                }
                else if (collection.Condition?.Trim() == "")
                {
                    rules.Condition = null;
                }
            }

            foreach (var (name, definition) in collection.Columns)
            {
                if (string.IsNullOrEmpty(definition.Type))
                {
                    throw new MigrationException($"Column ({name}) of table ({table}) has no corresponding type specified.");
                }

                var type = NormalizeType(definition.Type);
                var rule = new DataTransactionRule
                {
                    Type = type,
                    CurrentType = type,
                    Value = !alreadyPerformed ? definition.Value : null,
                    IsNew = !alreadyPerformed,
                    IsEnum = definition.IsEnum,
                    IsSequence = definition.IsSequence,
                    SequenceName = definition.SequenceName,
                    Inherits = definition.Inherits,
                };

                if (!_map[table].TryAdd(name, rule))
                {
                    throw new MigrationException($"A column with the name ({name}) already exists in table ({table}).");
                }
            }
        }
    }

    /// <summary>
    /// Merges the update guidelines into the <see cref="DataTransactionMap"/>.
    /// </summary>
    /// <param name="update">The update guidelines.</param>
    /// <param name="alreadyPerformed">A value indicating whether the guidelines have already been performed.</param>
    private void MergeUpdate(DataTransactionMap update, bool alreadyPerformed)
    {
        if (update is null || update.Count == 0)
            return;

        foreach (var (table, rules) in update)
        {
            EnsureTableExists("update", table);

            foreach (var (name, rule) in rules)
            {
                UpdateRule(table, name, rule, alreadyPerformed);

                if (!TransferDefinitions.TryGetValue(table, out var value))
                    continue;

                if (value.Columns.ContainsKey(name))
                    value.Columns[name] = rule.ColumnName!;
            }
        }
    }

    private void UpdateRule(string table, string columnName, DataTransactionRule rule, bool alreadyPerformed)
    {
        var record = GetRecord("update", table, columnName);

        var type = !string.IsNullOrEmpty(rule.Type)
            ? NormalizeType(rule.Type)
            : record.Value.Type;

        var column = new DataTransactionRule
        {
            Type = type,
            IsEnum = rule.IsEnum ?? record.Value.IsEnum,
            ColumnName = rule.ColumnName,

            Inherits = rule.Inherits?.Trim() == string.Empty
                ? rule.Inherits
                : null,

            CurrentType = !alreadyPerformed
                ? record.Value.CurrentType
                : type
        };

        if (!alreadyPerformed)
        {
            if (!string.IsNullOrEmpty(rule.Value))
            {
                column.Value = rule.Value.Contains(CastVariable) && !string.IsNullOrEmpty(column.Value)
                    ? rule.Value.Replace(CastVariable, column.Value)
                    : rule.Value;
            }

            _map[table][record.Key] = column;
        }
        else
        {
            _map[table].Remove(record.Key);
            _map[table][column.ColumnName ?? record.Key] = column;
        }
    }

    /// <summary>
    /// Merges the delete guidelines into the <see cref="DataTransactionMap"/>.
    /// </summary>
    /// <param name="delete">The delete guidelines.</param>
    private void MergeDelete(Dictionary<string, string[]> delete)
    {
        if (delete is null || delete.Count == 0)
            return;

        foreach (var (table, columns) in delete)
        {
            EnsureTableExists("delete", table);

            if (columns is not null && columns.Length != 0)
            {
                foreach (var column in columns)
                {
                    var record = GetRecord("delete", table, column);
                    _map[table].Remove(record.Key);

                    if (!TransferDefinitions.TryGetValue(table, out var value))
                        continue;

                    if (value.Columns.ContainsKey(column))
                    {
                        throw new MigrationException($"Column ({column}) of table ({table}) is set for transfer. Updating the transfer definition accordingly is recommended.");
                    }
                }
            }
            else
            {
                _map.Remove(table);
                _orderList.Remove(table);
                TransferDefinitions.Remove(table);
            }
        }
    }

    /// <summary>
    /// Generates a summary of the <see cref="DataTransactionMap"/>.
    /// </summary>
    /// <returns>A string containing the summary of the <see cref="DataTransactionMap"/>.</returns>
    public string GenerateSummary()
    {
        var sb = new StringBuilder();

        foreach (var (table, columns) in this)
        {
            sb.AppendLine(table);

            foreach (var (name, rule) in columns)
            {
                sb.Append("   " + name + ": " + rule?.Type);

                if (rule is not null)
                {
                    if (!string.IsNullOrEmpty(rule.ColumnName))
                        sb.Append(" => " + rule.ColumnName);

                    if (!string.IsNullOrEmpty(rule.Value))
                        sb.Append($", casted as ({rule.Value})");
                }

                sb.AppendLine();
            }

            sb.AppendLine();
        }

        if (TransferDefinitions.Count != 0)
        {
            sb.AppendLine("\n\n>> TRANSFER DEFINITIONS:\n");

            foreach (var (table, definition) in TransferDefinitions)
            {
                sb.AppendLine(table);

                foreach (var (name, type) in definition.Columns)
                {
                    sb.AppendLine("   " + name + ": " + type);
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, TransactionRuleDictionary>> IEnumerable<KeyValuePair<string, TransactionRuleDictionary>>.GetEnumerator()
        => new Enumerator(_orderList, _map);

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => new Enumerator(_orderList, _map);

    private void EnsureTableExists(string operation, string table)
    {
        if (!_map.ContainsKey(table))
        {
            throw new MigrationException($"Cannot {operation} undefined table ({table}).");
        }
    }

    private KeyValuePair<string, DataTransactionRule> GetRecord(string operation, string table, string column)
    {
        var record = _map[table].SingleOrDefault(record => record.Key == column || record.Value?.ColumnName == column);
        if (Equals(record, DefaultRecord))
        {
            throw new MigrationException($"Cannot {operation} undefined column ({column}) of table ({table}).");
        }

        return record;
    }

    private static string NormalizeType(string type)
    {
        return type.Trim().ToLower();
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, TransactionRuleDictionary> item)
    {
        Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public void Add(string key, TransactionRuleDictionary value)
    {
        _map.Add(key, value);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _map.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, TransactionRuleDictionary> item)
    {
        return _map.Contains(item);
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, TransactionRuleDictionary>>.CopyTo(KeyValuePair<string, TransactionRuleDictionary>[] array, int arrayIndex)
    {
    }

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index)
    {
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, TransactionRuleDictionary> item)
    {
        return _map.Remove(item.Key);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return _map.ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        return _map.Remove(key);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TransactionRuleDictionary value)
    {
        return _map.TryGetValue(key, out value);
    }

    private struct Enumerator : IEnumerator<KeyValuePair<string, TransactionRuleDictionary>>
    {
        private readonly List<string> _list;
        private readonly Dictionary<string, TransactionRuleDictionary> _dictionary;

        private int _index = 0;

        internal Enumerator(List<string> list, Dictionary<string, TransactionRuleDictionary> dictionary)
        {
            _list = list;
            _dictionary = dictionary;
        }

        /// <inheritdoc/>
        public KeyValuePair<string, TransactionRuleDictionary> Current { get; private set; } = default;

        /// <inheritdoc/>
        readonly object IEnumerator.Current => Current;

        /// <inheritdoc/>
        public readonly void Dispose()
        {
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_index == _list.Count)
                return false;

            var key = _list[_index++];
            Current = KeyValuePair.Create(key, _dictionary[key]);

            return true;
        }

        /// <inheritdoc/>
        readonly void IEnumerator.Reset()
        {
        }
    }
}
