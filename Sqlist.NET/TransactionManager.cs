using Sqlist.NET.Abstractions;
using Sqlist.NET.Utilities;

namespace Sqlist.NET
{
    /// <summary>
    ///     Provides a high-level access to the transaction API.
    /// </summary>
    public sealed class TransactionManager
    {
        private readonly DbCore _db;

        private IQueryStore _query;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbCore"/> for the transaction API.</param>
        public TransactionManager(DbCore db)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
            _query = db.Query();
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public void Begin()
        {
            _query = _db.Query();
            _db.BeginTransaction();
        }

        /// <summary>
        ///     Commits a database transaction.
        /// </summary>
        public void Commit()
        {
            _db.CommitTransaction();

            _query.Dispose();
            _query = null;
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public void Rollback()
        {
            _db.RollbackTransaction();

            _query.Dispose();
            _query = null;
        }
    }
}
