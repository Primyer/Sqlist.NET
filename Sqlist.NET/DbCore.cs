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
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sqlist.NET
{
    /// <summary>
    ///     Implements the <see cref="DbCoreBase"/> as a base of the provided querying API.
    /// </summary>
    public class DbCore : DbCoreBase
    {
        private readonly List<DbQuery> _queries = new List<DbQuery>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCore"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbCore(DbOptions options) : base(options)
        { }

        /// <summary>
        ///     Returns a new instance of the <see cref="IQueryStore"/>.
        /// </summary>
        /// <param name="presistent">The flag that indicates whether the instance is persistent within the <see cref="DbCore"/>'s scope.</param>
        /// <returns>A new instance of the <see cref="IQueryStore"/>.</returns>
        public virtual IQueryStore Query(bool presistent = false)
        {
            var qry = new DbQuery(this, presistent);

            if (presistent)
                _queries.Add(qry);

            return qry;
        }

        /// <summary>
        ///     Finalizes the given <paramref name="qry"/>.
        /// </summary>
        /// <param name="qry">The <see cref="DbQuery"/> to finalize.</param>
        internal virtual void FinalizeQuery(DbQuery qry)
        {
            ThrowIfDisposed();

            var removed = _queries.Remove(qry);
            qry.Dispose(removed && !_queries.Any());
        }

        /// <inheritdoc />
        public override Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            return WrapQuery(async () =>
            {
                using var cmd = CreateCommand(sql, prms, timeout, type);
                var result = await cmd.ExecuteNonQueryAsync();

                await cmd.Connection.CloseAsync();
                return result;
            });
        }

        /// <inheritdoc />
        public override Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            return WrapQuery(async () =>
            {
                using var cmd = CreateCommand(sql, prms, timeout);
                using var reader = await cmd.ExecuteReaderAsync();
                var result = DataMapper.Parse(reader, altr);

                await cmd.Connection.CloseAsync();
                return result;
            });
        }
    }
}
