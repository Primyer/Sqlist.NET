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
