using System;
using System.Collections.Generic;
using System.Data;

namespace Sqlist.NET
{
    internal sealed class FirebirdTypeMapper : TypeMapper
    {
        private static readonly Lazy<FirebirdTypeMapper> Instance = new(() => new FirebirdTypeMapper());

        // Common SQL name -> CLR type mapping for Firebird
        private static readonly Dictionary<string, Type> NameToClr = new(StringComparer.OrdinalIgnoreCase)
        {
            ["smallint"] = typeof(short),
            ["int2"] = typeof(short),
            ["integer"] = typeof(int),
            ["int"] = typeof(int),
            ["int4"] = typeof(int),
            ["bigint"] = typeof(long),
            ["int8"] = typeof(long),

            ["decimal"] = typeof(decimal),
            ["numeric"] = typeof(decimal),

            ["float"] = typeof(double),
            ["double"] = typeof(double),
            ["double precision"] = typeof(double),
            ["real"] = typeof(float),

            ["boolean"] = typeof(bool),
            ["bool"] = typeof(bool),

            ["char"] = typeof(string),
            ["character"] = typeof(string),
            ["varchar"] = typeof(string),
            ["character varying"] = typeof(string),
            ["text"] = typeof(string),

            ["blob"] = typeof(byte[]),

            ["date"] = typeof(DateTime),
            ["time"] = typeof(TimeSpan),
            ["timestamp"] = typeof(DateTime),
            ["timestamptz"] = typeof(DateTimeOffset),

            ["uuid"] = typeof(Guid),
            ["guid"] = typeof(Guid)
        };

        // DbType -> provider SQL name used when emitting types
        private static readonly Dictionary<DbType, string> DbTypeToName = new()
        {
            [DbType.Int16] = "smallint",
            [DbType.Int32] = "int",
            [DbType.Int64] = "bigint",
            [DbType.Decimal] = "decimal",
            [DbType.Double] = "double precision",
            [DbType.Single] = "real",
            [DbType.String] = "varchar",
            [DbType.Binary] = "blob",
            [DbType.Date] = "date",
            [DbType.Time] = "time",
            [DbType.DateTime] = "timestamp",
            [DbType.DateTimeOffset] = "timestamptz",
            [DbType.Boolean] = "boolean",
        };

        private FirebirdTypeMapper() { }

        public static FirebirdTypeMapper Default => Instance.Value;

        private static string NormalizeType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return string.Empty;

            var t = type.Trim();
            var idx = t.IndexOfAny(new[] { '(', '[' });
            if (idx != -1)
                t = t[..idx];

            return t.Trim().ToLowerInvariant();
        }

        public override Type GetType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return typeof(object);

            var normalized = NormalizeType(name);

            if (NameToClr.TryGetValue(normalized, out var clr))
                return name.Contains('[') ? clr.MakeArrayType() : clr;

            return typeof(object);
        }

        public override string TypeName(DbType dbType)
        {
            return DbTypeToName.GetValueOrDefault(dbType, dbType.ToString());
        }
    }
}
