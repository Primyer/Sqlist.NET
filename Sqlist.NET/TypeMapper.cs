using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sqlist.NET
{
    public abstract class TypeMapper
    {
        private static readonly Dictionary<DbType, Type> ClrTypes = new Dictionary<DbType, Type>
        {
            [DbType.AnsiString] = typeof(string),
            [DbType.Binary] = typeof(byte[]),
            [DbType.Byte] = typeof(byte),
            [DbType.Boolean] = typeof(bool),
            [DbType.Currency] = typeof(decimal),
            [DbType.Date] = typeof(DateTime),
            [DbType.DateTime] = typeof(DateTime),
            [DbType.Decimal] = typeof(decimal),
            [DbType.Double] = typeof(double),
            [DbType.Guid] = typeof(Guid),
            [DbType.Int16] = typeof(short),
            [DbType.Int32] = typeof(int),
            [DbType.Int64] = typeof(long),
            [DbType.Object] = typeof(object),
            [DbType.SByte] = typeof(sbyte),
            [DbType.Single] = typeof(float),
            [DbType.String] = typeof(string),
            [DbType.Time] = typeof(TimeSpan),
            [DbType.UInt16] = typeof(ushort),
            [DbType.UInt32] = typeof(uint),
            [DbType.UInt64] = typeof(ulong),
            [DbType.VarNumeric] = typeof(decimal),
            [DbType.AnsiStringFixedLength] = typeof(string),
            [DbType.StringFixedLength] = typeof(string),
            [DbType.Xml] = typeof(string),
            [DbType.DateTime2] = typeof(DateTime),
            [DbType.DateTimeOffset] = typeof(DateTimeOffset)
        };

        private static readonly Dictionary<Type, DbType> DbTypes = new Dictionary<Type, DbType>
        {
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte)] = DbType.Byte,
            [typeof(bool)] = DbType.Boolean,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(double)] = DbType.Double,
            [typeof(Guid)] = DbType.Guid,
            [typeof(short)] = DbType.Int16,
            [typeof(int)] = DbType.Int32,
            [typeof(long)] = DbType.Int64,
            [typeof(object)] = DbType.Object,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(float)] = DbType.Single,
            [typeof(string)] = DbType.String,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(uint)] = DbType.UInt32,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset
        };

        /// <summary>
        ///     Returns the name of the corresponding type of the DB provider.
        /// </summary>
        /// <typeparam name="T">The CLR type to match up.</typeparam>
        /// <returns>The name of the corresponding type of the DB provider.</returns>
        /// <exception cref="NotSupportedException" />
        public string TypeName<T>()
        {
            var dbType = ToDbType(typeof(T));
            return TypeName(dbType);
        }

        /// <summary>
        ///     Returns the name of the corresponding type of the DB provider.
        /// </summary>
        /// <param name="dbType">The ADO.NET <see cref="DbType"/> to match up.</param>
        /// <returns>The name of the corresponding type of the DB provider.</returns>
        /// <exception cref="NotSupportedException" />
        public abstract string TypeName(DbType dbType);

        /// <summary>
        ///     Returns the name of the corresponding type of the DB provider.
        /// </summary>
        /// <param name="name">The name of the DB provider type.</param>
        /// <returns>The name of the corresponding type of the DB provider.</returns>
        /// <exception cref="NotSupportedException" />
        public DbType GetDbType(string name)
        {
            var type = GetType(name);
            return DbTypes[type];
        }

        /// <summary>
        ///     Returns the corresponding CLR type.
        /// </summary>
        /// <param name="name">The name of the DB provider type.</param>
        /// <returns>The corresponding CLR type.</returns>
        /// <exception cref="NotSupportedException" />
        public abstract Type GetType(string name);

        /// <summary>
        ///     Returns the corresponding type of the DB provider.
        /// </summary>
        /// <param name="type">The CLR type to match up.</param>
        /// <returns>The corresponding type of the DB provider</returns>
        public DbType ToDbType(Type type)
        {
            return DbTypes.ContainsKey(type)
                ? DbTypes[type]
                : DbType.Object;
        }

        /// <summary>
        ///     Returns the corresponding CLR type.
        /// </summary>
        /// <param name="type">The <see cref="DbType"/> to match up.</param>
        /// <returns>The corresponding CLR type.</returns>
        public Type FromDbType(DbType type)
        {
            return ClrTypes[type];
        }
    }
}
