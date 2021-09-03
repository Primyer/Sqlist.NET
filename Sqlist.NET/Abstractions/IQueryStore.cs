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

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Sqlist.NET.Abstractions
{
    /// <summary>
    ///     Provides the API to query against a data source.
    /// </summary>
    public interface IQueryStore : IDisposable
    {
        /// <summary>
        ///     Executes the specified <paramref name="sql"/> statement against the data source, as an asynchronous operation.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the number of rows affected.
        /// </returns>
        Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null);
        /// <summary>
        ///     Executes the specified <paramref name="sql"/> statement against the data source.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>The number of rows affected.</returns>
        int Execute(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source,
        ///     and returns an <see cref="IEnumerable{T}"/> as the result, as an asynchronous operation.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="altr">The action to modify the generated models within the mapping process.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> as result.
        /// </returns>
        Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source,
        ///     and returns an <see cref="IEnumerable{T}"/> as the result.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> as result.</returns>
        IEnumerable<T> Retrieve<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source, and returns the first row of
        ///     the result, if any; otherwise, <see langword="null" />.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing an first result of the result, if any.
        /// </returns>
        Task<T> FirstOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source, and returns the first row of
        ///     the result, if any; otherwise, <see langword="null" />.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>The first row of the result, if any.</returns>
        T FirstOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source, and returns the result if it only
        ///     a single row; otherwise, <see langword="null" />.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing an returns the result if it only
        ///     a single row; otherwise, <see langword="null" />
        /// </returns>
        Task<T> SingleOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source, and returns the result if it only
        ///     a single row; otherwise, <see langword="null" />.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>The result if it only a single row; otherwise, <see langword="null" />.</returns>
        T SingleOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null);
    }
}
