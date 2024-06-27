using System;

namespace Sqlist.NET.Migration.Exceptions
{
    /// <summary>
    ///     Represents errors that occur during database migration.
    /// </summary>
    public class MigrationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationException"/> class.
        /// </summary>
        public MigrationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationException"/> class.
        /// </summary>
        /// <inheritdoc />
        public MigrationException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationException"/> class.
        /// </summary>
        /// <inheritdoc />
        public MigrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
