using System;
using System.Collections.Generic;
using System.Data;

namespace Sqlist.NET
{
    public sealed class TypeMapper
    {
        private static readonly Dictionary<string, DbType> NativeTypes = new Dictionary<string, DbType>
        {
            [typeof(byte).Name] = DbType.Byte,
            [typeof(bool).Name] = DbType.Boolean,
            [typeof(byte[]).Name] = DbType.Binary,
            [typeof(DateTime).Name] = DbType.DateTime,
            [typeof(DateTimeOffset).Name] = DbType.DateTimeOffset,
            [typeof(decimal).Name] = DbType.Decimal,
            [typeof(double).Name] = DbType.Double,
            [typeof(float).Name] = DbType.Single,
            [typeof(Guid).Name] = DbType.Guid,
            [typeof(short).Name] = DbType.Int16,
            [typeof(int).Name] = DbType.Int32,
            [typeof(long).Name] = DbType.Int64,
            [typeof(object).Name] = DbType.Object,
            [typeof(string).Name] = DbType.String,
            [typeof(TimeSpan).Name] = DbType.Time,
            [typeof(ushort).Name] = DbType.UInt16,
            [typeof(uint).Name] = DbType.UInt32,
            [typeof(ulong).Name] = DbType.UInt64
        };

        public static TypeMapper Instance => new TypeMapper();

        private TypeMapper()
        { }

        public DbType ToDbType(Type type)
        {
            return NativeTypes.ContainsKey(type.Name)
                ? NativeTypes[type.Name]
                : DbType.Object;
        }

        public DbType ToDbType(int typeInt)
        {
            return Enum.TryParse(typeof(DbType), typeInt.ToString(), out var result)
                ? (DbType)result
                : DbType.Object;
        }


        public T ToCustomDbType<T>(DbType type) where T : Enum
        {
            if (Enum.TryParse(typeof(DbType), ((int)type).ToString(), out var result))
                return (T)result;

            else throw new NotSupportedException($"The parameter type DbType.{type} isn't supported by {typeof(T).Name}");
        }
    }
}
