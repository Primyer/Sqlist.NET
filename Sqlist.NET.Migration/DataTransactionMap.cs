﻿using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqlist.NET.Migration
{
    public class DataTransactionMap : Dictionary<string, TransactionRuleDictionary>
    {
        private const string Tab = "   ";
        public const string CastVariable = "{column}";

        static readonly KeyValuePair<string, DataTransactionRule> DefaultRecord = default;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataTransactionMap"/> class.
        /// </summary>
        public DataTransactionMap()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataTransactionMap"/> class.
        /// </summary>
        /// <param name="phases">The <see cref="MigrationPhase"/> collection to be merged.</param>
        /// <param name="currentVersion">The version of the current DB schema.</param>
        public DataTransactionMap(IEnumerable<MigrationPhase> phases, Version? currentVersion = null)
        {
            foreach (var phase in phases)
                Merge(phase, currentVersion);
        }

        public void Merge(MigrationPhase phase, Version? currentVersion = null)
        {
            var performed = currentVersion != null && currentVersion >= phase.Version;

            MergeCreate(phase.Guidelines.Create, performed);
            MergeUpdate(phase.Guidelines.Update, performed);
            MergeDelete(phase.Guidelines.Delete);
        }

        public void MergeCreate(Dictionary<string, ColumnsDefinition> create, bool alreadyPerformed)
        {
            if (!(create?.Any() ?? false))
                return;

            foreach (var (table, collection) in create)
            {
                if (!ContainsKey(table))
                    this[table] = new TransactionRuleDictionary { Condition = collection.Condition };
                else
                {
                    if (collection.Condition?.Trim() == "")
                        this[table].Condition = null;
                    
                    else if (!string.IsNullOrWhiteSpace(collection.Condition))
                        this[table].Condition = collection.Condition;
                }

                foreach (var (name, definition) in collection.Columns)
                {
                    if (string.IsNullOrEmpty(definition.Type))
                        throw new InvalidOperationException($"Column ({name}) of table ({table}) has no corresponding type specified.");

                    var type = NormalizeType(definition.Type);

                    this[table].Add(name, new DataTransactionRule
                    {
                        Type = type,
                        CurrentType = type,
                        Value = !alreadyPerformed ? definition.Value : null,
                        IsNew = !alreadyPerformed,
                        IsEnum = definition.IsEnum
                    });
                }
            }
        }

        public void MergeUpdate(DataTransactionMap update, bool alreadyPerformed)
        {
            if (!(update?.Any() ?? false))
                return;

            foreach (var (table, columns) in update)
            {
                EnsureTableExists("update", table);

                foreach (var (name, rule) in columns)
                {
                    var record = GetRecord("update", table, name);

                    var type = !string.IsNullOrEmpty(rule.Type)
                        ? NormalizeType(rule.Type)
                        : record.Value.Type;

                    var column = new DataTransactionRule
                    {
                        Type = type,
                        IsEnum = rule.IsEnum ?? record.Value.IsEnum,
                        ColumnName = rule.ColumnName,
                        
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

                        this[table][record.Key] = column;
                    }
                    else
                    {
                        this[table].Remove(record.Key);
                        this[table][column.ColumnName ?? record.Key] = column;
                    }
                }
            }
        }

        public void MergeDelete(Dictionary<string, string[]> delete)
        {
            if (!(delete?.Any() ?? false))
                return;

            foreach (var (table, columns) in delete)
            {
                EnsureTableExists("delete", table);

                if (columns?.Any() ?? false)
                {
                    foreach (var column in columns)
                    {
                        var record = GetRecord("delete", table, column);
                        this[table].Remove(record.Key);
                    }
                }
                else Remove(table);
            }
        }

        public string GenerateSummary()
        {
            var sb = new StringBuilder();

            foreach (var (table, columns) in this)
            {
                sb.AppendLine(table);

                foreach (var (name, rule) in columns)
                {
                    sb.Append(Tab + name + ": " + rule.Type);

                    if (rule is not null)
                    {
                        if (!string.IsNullOrEmpty(rule.ColumnName))
                            sb.Append(" => " + rule.ColumnName);

                        if (!string.IsNullOrEmpty(rule.Value))
                            sb.Append($", casted as ({rule.Value})");
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private void EnsureTableExists(string operation, string table)
        {
            if (!ContainsKey(table))
                throw new InvalidOperationException($"Cannot {operation} undefined table ({table}).");
        }

        private KeyValuePair<string, DataTransactionRule> GetRecord(string operation, string table, string column)
        {
            var record = this[table].SingleOrDefault(record => record.Key == column || record.Value?.ColumnName == column);
            if (record.Equals(DefaultRecord))
                throw new InvalidOperationException($"Cannot {operation} undefined column ({column}) of table ({table}).");

            return record;
        }

        private static string NormalizeType(string type)
        {
            return type.Trim().ToLower();
        }
    }
}