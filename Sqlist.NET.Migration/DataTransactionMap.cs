using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqlist.NET.Migration
{
    public class DataTransactionMap : Dictionary<string, TransactionRuleDictionary>
    {
        private const string CastVariable = "{Column}";

        static readonly KeyValuePair<string, DataTransactionRule> DefaultRecord = default;

        public void Merge(MigrationPhase phase, Version? currentVersion = null)
        {
            var isNew = currentVersion is null || currentVersion < phase.Version;

            MergeCreate(phase.Guidelines.Create, isNew);
            MergeUpdate(phase.Guidelines.Update);
            MergeDelete(phase.Guidelines.Delete);
        }

        public void MergeCreate(Dictionary<string, DefinitionCollection> create, bool isNew)
        {
            if (!(create?.Any() ?? false))
                return;

            foreach (var (table, columns) in create)
            {
                if (!ContainsKey(table))
                    this[table] = new TransactionRuleDictionary();

                foreach (var (name, type) in columns)
                {
                    if (string.IsNullOrEmpty(type))
                        throw new InvalidOperationException($"Column ({name}) of table ({table}) has no corresponding type specified.");

                    this[table].Add(name, new DataTransactionRule
                    {
                        Type = type,
                        IsNew = isNew
                    });
                }
            }
        }

        public void MergeUpdate(DataTransactionMap update)
        {
            if (!(update?.Any() ?? false))
                return;

            foreach (var (table, columns) in update)
            {
                EnsureTableExists("update", table);

                foreach (var (name, rule) in columns)
                {
                    var record = GetRecord("update", table, name);
                    var column = new DataTransactionRule
                    {
                        ColumnName = rule.ColumnName,
                        Type = record.Value.Type
                    };

                    column.Cast = !string.IsNullOrEmpty(column.Cast)
                        ? rule.Cast?.Replace(CastVariable, column.Cast)
                        : rule.Cast;

                    this[table][record.Key] = column;
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
    }
}
