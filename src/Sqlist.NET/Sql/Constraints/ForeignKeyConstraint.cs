using Sqlist.NET.Sql.Metadata;
using Sqlist.NET.Sql.Partial;

namespace Sqlist.NET.Sql.Constraints;
public class ForeignKeyConstraint : ConstraintBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ForeignKeyConstraint"/> class.
    /// </summary>
    /// <inheritdoc />
    public ForeignKeyConstraint(string[] columns) : base(columns)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForeignKeyConstraint"/> class.
    /// </summary>
    /// <inheritdoc />
    public ForeignKeyConstraint(string name, params string[] columns) : base(name, columns)
    {
    }


    /// <summary>
    ///     Gets or sets the foreign key reference.
    /// </summary>
    public Reference References { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="ReferencialAction"/> which to be applied when a the reference updated.
    /// </summary>
    public ReferencialAction OnUpdate { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="ReferencialAction"/> which to be applied when a the reference deleted.
    /// </summary>
    public ReferencialAction OnDelete { get; set; }
}