namespace Sqlist.NET.Sql.Constraints
{
    public class PrimaryKeyConstraint : ConstraintBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryKeyConstraint"/> class.
        /// </summary>
        /// <inheritdoc />
        public PrimaryKeyConstraint(string[] columns) : base(columns)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryKeyConstraint"/> class.
        /// </summary>
        /// <inheritdoc />
        public PrimaryKeyConstraint(string name, params string[] columns) : base(name, columns)
        {
        }
    }
}
