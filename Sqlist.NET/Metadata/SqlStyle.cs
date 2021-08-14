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

namespace Sqlist.NET
{
    public enum SqlStyle
    {

        /// <summary>
        ///     Represents the SQL syntax that's used in PostgreSQL databases.
        /// </summary>
        PL_pgSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in Oracle databases.
        /// </summary>
        OracleSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in Firebird databases.
        /// </summary>
        FirebirdSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in SQL Server databases.
        /// </summary>
        MSSQL = 1,

        /// <summary>
        ///     Represents the SQL syntax that's used in databases like PHP My Admin.
        /// </summary>
        MySQL = 2
    }
}
