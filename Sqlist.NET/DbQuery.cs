using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;

namespace Sqlist.NET
{
    /// <summary>
    ///     Saves the connection state allowing the reusing of that particular connection
    ///     in a chain of queries.
    ///     <para>
    ///         Note that ADO.NET -being the underlying core for this API- supports connection pooling.
    ///         So, it's recommended to reuse the same connection only when it's needed, since reusing
    ///         the same connection on higher scopes makes it harder to manage asyncronization on the long term.
    ///     </para>
    /// </summary>
    public class DbQuery<T> : IDisposable where T : DbCoreBase
    {
        private readonly DbCoreBase _db;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbQuery"/> class.
        /// </summary>
        /// <param name="db">The source <see cref="DbCoreBase"/>.</param>
        public DbQuery(DbCoreBase db)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _db.Connection.Dispose();
            _db.Connection = null;
        }


    }
}
