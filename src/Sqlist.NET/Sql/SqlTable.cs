using Sqlist.NET.Sql.Constraints;

namespace Sqlist.NET.Sql;

/// <summary>
///     Initializes a new instance of the <see cref="SqlTable"/> class.
/// </summary>
/// <param name="schema">The schema where the table is to be created.</param>
/// <param name="name">The name of the table.</param>
/// <param name="ifNotExists">The flag indicating whether the create the table to be, only if it doesn't already exist.</param>
public class SqlTable(string? schema, string name, bool ifNotExists = false)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlTable"/> class.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <param name="ifNotExists">The flag indicating whether the create the table to be, only if it doesn't already exist.</param>
    public SqlTable(string name, bool ifNotExists = false) : this(null, name, ifNotExists)
    {
    }

    /// <summary>
    ///     Gets or sets the schema where the table is to be created.
    /// </summary>
    public string? Schema { get; set; } = schema;

    /// <summary>
    ///     Gets or sets the name of the table.
    /// </summary>
    public string? Name { get; set; } = name;

    /// <summary>
    ///     Gets or sets the flag indicating whether the create the table to be, only if it doesn't already exist.
    /// </summary>
    public bool IfNotExists { get; set; } = ifNotExists;

    /// <summary>
    ///     Gets or sets the collection of the columns, which are to be created alog with the table.
    /// </summary>
    public IList<SqlColumn> Columns { get; set; } = [];

    /// <summary>
    ///     Gets or sets the constraints associated with the table.
    /// </summary>
    public TableConstraints Constraints { get; set; } = new TableConstraints();
}