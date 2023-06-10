using System.Linq;
using System;

namespace Sqlist.NET.Sql.Constraints
{
    public abstract class ConstraintBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConstraintBase"/> class.
        /// </summary>
        /// <param name="columns">The columns which to be constrainted.</param>
        public ConstraintBase(string[] columns)
        {
            if (columns is null)
                throw new ArgumentNullException(nameof(columns));

            if (!columns.Any())
                throw new ArgumentException(nameof(columns) + " cannot be empty.");

            Columns = columns;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryKeyConstraint"/> class.
        /// </summary>
        /// <param name="name">The custom constraint name.</param>
        /// <param name="columns">The columns which to be constrainted.</param>
        public ConstraintBase(string name, params string[] columns) : this(columns)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the custom constraint name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        ///     Gets the columns which to be constrainted.
        /// </summary>
        public string[] Columns { get; }
    }
}
