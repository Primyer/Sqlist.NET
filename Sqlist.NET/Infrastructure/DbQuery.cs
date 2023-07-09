﻿using Sqlist.NET.Abstractions;
using Sqlist.NET.Utilities;

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Sqlist.NET.Infrastructure
{
    /// <summary>
    ///     <see cref="DbQuery"/> allows reusing a single database connection. Nevertheless, it allows
    ///     the same connection to be reused by any <see cref="DbQuery"/> nested in between the initialization
    ///     and the release of the highest <see cref="DbQuery"/> instance.
    /// </summary>
    /// <remarks>
    ///     The connection in this case is never closed until the heighest query is closed. Then, the connection
    ///     is to be closed and returned to the connection pool to be reused later. This connection keeps
    ///     alive along the lifetime of the containing <see cref="DbContextBase"/> instance and to be disposed with it.
    /// </remarks>
    public class DbQuery : QueryStore
    {
        private readonly bool _initTransaction;
        private readonly DbContextBase _db;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbQuery"/> class.
        /// </summary>
        /// <param name="db">The source <see cref="DbContextBase"/>.</param>
        /// <param name="initTransaction">The flag indicating whether this query should initialize a transaction.</param>
        internal DbQuery(DbContextBase db, bool initTransaction = false) : base(db.Options)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
            _initTransaction = initTransaction;
        }

        /// <summary>
        ///     Gets the ID of this query.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the flag indicating whether to terminate the connection at the end
        ///     of this query.
        /// </summary>
        public bool TerminateConnection { get; set; }

        /// <inheritdoc />
        protected override async ValueTask<DbConnection> GetConnectionAsync()
        {
            await _db.InvokeConnectionAsync();

            if (_initTransaction)
                await _db.BeginTransactionAsync();

            return _db.Connection!;
        }

        /// <inheritdoc />
        public override Command CreateCommand()
        {
            return _db.CreateCommand();
        }

        /// <inheritdoc />
        public override Command CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return _db.CreateCommand(sql, prms, timeout, type);
        }
    }
}