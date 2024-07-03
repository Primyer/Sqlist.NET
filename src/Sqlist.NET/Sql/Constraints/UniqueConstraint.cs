namespace Sqlist.NET.Sql.Constraints;
public class UniqueConstraint : ConstraintBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UniqueConstraint"/> class.
    /// </summary>
    /// <inheritdoc />
    public UniqueConstraint(string[] columns) : base(columns)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UniqueConstraint"/> class.
    /// </summary>
    /// <inheritdoc />
    public UniqueConstraint(string name, params string[] columns) : base(name, columns)
    {
    }
}