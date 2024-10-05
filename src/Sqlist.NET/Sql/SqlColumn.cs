namespace Sqlist.NET.Sql;

public class SqlColumn
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlColumn"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="type">The type of the column.</param>
    /// <param name="notNull">The flag indicating whether the column cannot be null.</param>
    public SqlColumn(string name, string type, bool notNull) : this(name, type)
    {
        NotNull = notNull;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlColumn"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="type">The type of the column.</param>
    /// <param name="defaultValue">The default value of the column.</param>
    public SqlColumn(string name, string type, string defaultValue) : this(name, type)
    {
        Default = defaultValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlColumn"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="type">The type of the column.</param>
    public SqlColumn(string name, string type)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        
        Name = name;
        Type = type;
    }

    /// <summary>
    ///     Gets or sets the name of the column.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets or sets the type of the column.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Gets or sets the default value of the column.
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    ///     Gets or sets the flag indicating whether the column cannot be null.
    /// </summary>
    public bool NotNull { get; set; }

    /// <summary>
    ///     Gets or sets the flag indicating whether to constraint this column as primary key.
    /// </summary>
    public bool PrimaryKey { get; set; }

    /// <summary>
    ///     Gets or sets the flag indicating whether to constraint this column as auto-increment.
    /// </summary>
    public bool AutoIncrement { get; init; }

    /// <summary>
    ///     Gets or sets the flag indicating whether to constraint this column as unique.
    /// </summary>
    public bool Unique { get; set; }

    /// <summary>
    ///     Gets or sets conditional clause to check constraint this column.
    /// </summary>
    public ConditionalClause? Check { get; set; }
}