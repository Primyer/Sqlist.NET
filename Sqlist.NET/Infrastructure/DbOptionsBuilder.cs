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

using Sqlist.NET.Utilities;

using System;
using System.Data.Common;
using System.Reflection;

namespace Sqlist.NET.Infrastructure
{
    /// <summary>
    ///     Provides the API to configure <see cref="DbOptions"/>.
    /// </summary>
    public class DbOptionsBuilder
    {
        private readonly DbOptions _opts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbOptionsBuilder"/> class.
        /// </summary>
        public DbOptionsBuilder()
        {
            _opts = new DbOptions();
        }

        /// <summary>
        ///     Gets the configured instance of <see cref="DbOptions"/>.
        /// </summary>
        internal DbOptions Options => _opts;

        /// <summary>
        ///     Sets the <see cref="DbProviderFactory"/> the be used as a source of database connection.
        /// </summary>
        /// <typeparam name="T">The type derived from the <see cref="DbProviderFactory"/> class.</typeparam>
        /// <param name="provider">The provider factory to be used.</param>
        public void SetDbProvider<T>(T provider) where T : DbProviderFactory
        {
            Check.NotNull(provider, nameof(provider));

            _opts.DbProviderFactory = provider;
        }

        /// <summary>
        ///     Sets the basic syntax style to be used in generating SQL statements.
        /// </summary>
        /// <param name="style">The SQL syntax style.</param>
        public void SetSqlSyntaxStyle(SqlStyle style)
        {
            _opts.SqlStyle = style;
        }

        /// <summary>
        ///     Sets the connection string for the target database.
        /// </summary>
        /// <param name="connStr">The database connection string.</param>
        public void SetConnectionString(string connStr)
        {
            Check.NotNullOrEmpty(connStr, nameof(connStr));

            _opts.ConnectionString = connStr;
        }

        /// <summary>
        ///     Sets the version that represents the target database.
        ///     <para>
        ///         The database version represents the starting point of migration for the compiled binary.
        ///     </para>
        /// </summary>
        /// <param name="version">The database version.</param>
        public void SetDbVersion(Version version)
        {
            Check.NotNull(version, nameof(version));

            _opts.DbVersion = version;
        }

        /// <summary>
        ///     Sets the assembly reference where the migrations belong.
        /// </summary>
        /// <param name="assembly">The assembly reference.</param>
        public void SetMigrationAssembly(Assembly assembly)
        {
            Check.NotNull(assembly, nameof(assembly));

            _opts.MigrationSource = assembly;
        }

        /// <summary>
        ///     Enables sensitive information logging such as command parameters.
        /// </summary>
        public void EnableSensitiveLogging()
        {
            _opts.EnableSensitiveLogging = true;
        }

        /// <summary>
        ///     Enables analysis options.
        /// </summary>
        public void EnableAnalysis()
        {
            _opts.EnableAnalysis = true;
        }

        /// <summary>
        ///     Sets the mapping orientation.
        /// </summary>
        /// <param name="orientation">The mapping orientation to set.</param>
        public void SetMappingOrientation(MappingOrientation orientation)
        {
            _opts.MappingOrientation = orientation;
        }
    }
}
