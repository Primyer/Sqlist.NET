using System.Collections.Generic;

namespace Sqlist.NET.Sql.Constraints
{
    public class TableConstraints
    {
        /// <summary>
        ///     Gets or sets the primary-key constraint of a table.
        /// </summary>
        public PrimaryKeyConstraint? PrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the collection of foregin-key constarints of a table.
        /// </summary>
        public IList<ForeignKeyConstraint> ForeignKeys { get; set; } = [];

        /// <summary>
        ///     Gets or sets the collection of unqiue constarints of a table.
        /// </summary>
        public IList<UniqueConstraint> Uniques { get; set; } = [];

        /// <summary>
        ///     Gets or sets the collection of check constraints of a table.
        /// </summary>
        public IList<CheckConstraint> Checks { get; set; } = [];
    }
}
