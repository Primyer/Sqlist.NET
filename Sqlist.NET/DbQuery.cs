#region License
// Copyright (c) 2021, Saleh Kawaf Kulla
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Sqlist.NET.Abstractions;
using Sqlist.NET.Common;
using Sqlist.NET.Utilities;

using System;

namespace Sqlist.NET
{
    /// <summary>
    ///     <see cref="DbQuery"/> allows reusing a single database connection. Nevertheless, it allows
    ///     the same connection to be reused by any <see cref="DbQuery"/> nested in between the initialization
    ///     and the release of the highest <see cref="DbQuery"/> instance.
    /// </summary>
    /// <remarks>
    ///     The connection in this case is never closed until the heighest query is closed. Then, the connection
    ///     is to be closed and returned to the connection pool to be reused later. This connection keeps
    ///     alive along the lifetime of the containing <see cref="DbCore"/> instance and to be disposed with it.
    /// </remarks>
    public class DbQuery : QueryStore
    {
        private readonly DbCore _db;

        private bool _disposed;
        private int _delayQueueLength = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbQuery"/> class.
        /// </summary>
        /// <param name="db">The source <see cref="DbCore"/>.</param>
        internal DbQuery(DbCore db) : base(db.Options, db.Logger)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
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
        public override void Dispose()
        {
            if (!_disposed && _delayQueueLength == 0)
            {
                _db.FinalizeQuery(this);
                _disposed = true;
            }
        }

        protected override Action OnCommandCompleted()
        {
            _delayQueueLength++;

            return () =>
            {
                if (--_delayQueueLength == 0)
                    Dispose();
            };
        }

        protected override DbConnection GetConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DbQuery));

            return _db.Connection;
        }
    }
}