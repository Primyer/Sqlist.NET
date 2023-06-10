using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System.Threading.Tasks;

namespace Sqlist.NET
{
    /// <summary>
    ///     Provides a high-level access to the transaction API.
    /// </summary>
    public sealed class TransactionManager
    {
        private readonly DbContextBase _db;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbContextBase"/> for the transaction API.</param>
        public TransactionManager(DbContextBase db)
        {
            Check.NotNull(db, nameof(db));
            _db = db;
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public void Begin()
        {
            BeginAsync().Wait();
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public Task BeginAsync()
        {
            return _db.BeginTransactionAsync();
        }

        /// <summary>
        ///     Commits a database transaction.
        /// </summary>
        public void Commit()
        {
            CommitAsync().Wait();
        }

        /// <summary>
        ///     Commits a database transaction.
        /// </summary>
        public async Task CommitAsync()
        {
            await _db.CommitTransactionAsync();
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public void Rollback()
        {
            RollbackAsync().Wait();
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public async Task RollbackAsync()
        {
            await _db.RollbackTransactionAsync();
        }
    }
}
