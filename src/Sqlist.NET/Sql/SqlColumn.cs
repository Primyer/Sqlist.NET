namespace Sqlist.NET.Sql;

/// <summary>
///     Initializes a new instance of the <see cref="SqlColumn"/> class.
/// </summary>
/// <param name="name">The name of the column.</param>
/// <param name="type">The type of the column.</param>
public class SqlColumn(string name, string type)
{

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
    /// <param name="notNull">The flag indicating whether the column cannot be null.</param>
    /// <param name="defaultValue">The default value of the column.</param>
    public SqlColumn(string name, string type, bool notNull, string defaultValue) : this(name, type, defaultValue)
    {
        NotNull = notNull;
    }

    /// <summary>
    ///     Gets or sets the name of the column.
    /// </summary>
    public string? Name { get; set; } = name;

    /// <summary>
    ///     Gets or sets the type of the column.
    /// </summary>
    public string? Type { get; set; } = type;

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
    ///     Gets or sets the flag indicating whether to constraint this column as unique.
    /// </summary>
    public bool Unique { get; set; }

    /// <summary>
    ///     Gets or sets conditional clause to check constraint this column.
    /// </summary>
    public ConditionalClause? Check { get; set; }
}