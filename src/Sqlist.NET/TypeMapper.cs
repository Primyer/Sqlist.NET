using System.Data;

namespace Sqlist.NET;
public abstract class TypeMapper : ITypeMapper
{
    private static readonly Dictionary<DbType, Type> ClrTypes = new()
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

    private static readonly Dictionary<Type, DbType> DbTypes = new()
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

    /// <inheritdoc />
    public string TypeName<T>(uint? precision = null, int? scale = null)
    {
        if (scale.HasValue && !precision.HasValue)
        {
            throw new InvalidOperationException("Precision must be specified when scale is specified.");
        }
        
        var type = ToDbType(typeof(T));
        var name = TypeName(type);
        
        if (string.Join(',', precision, scale) is { Length: > 0 } pair)
        {
            name += $" ({pair})";
        }
        return name;
    }

    /// <inheritdoc />
    public DbType GetDbType(string name)
    {
        var type = GetType(name);
        return DbTypes[type];
    }

    /// <inheritdoc />
    public DbType ToDbType(Type type)
    {
        return DbTypes.GetValueOrDefault(type, DbType.Object);
    }

    /// <inheritdoc />
    public Type FromDbType(DbType type)
    {
        return ClrTypes[type];
    }

    /// <inheritdoc />
    public abstract Type GetType(string name);

    /// <inheritdoc />
    public abstract string TypeName(DbType dbType);
}