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
    ///     Provides the configuration options needed for a regular Sqlist API.
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbOptions"/> class.
        /// </summary>
        internal DbOptions()
        { }

        /// <summary>
        ///     Gets or sets the factory instance to create database connections with.
        /// </summary>
        public DbProviderFactory DbProviderFactory { get; set; }

        /// <summary>
        ///     Gets or sets the connection string to the target database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the version of the database.
        /// </summary>
        public Version DbVersion { get; set; }

        /// <summary>
        ///     Gets or sets the assembly reference where the migrations belong.
        /// </summary>
        public Assembly MigrationSource { get; set; }

        /// <summary>
        ///     Gets or sets the style of syntax which SQL statements should be based on.
        /// </summary>
        public SqlStyle SqlStyle { get; set; }

        /// <summary>
        ///     Gets or sets the flag indicating whether to log sensitive information such as command parameters.
        /// </summary>
        public bool EnableSensitiveLogging { get; set; }

        // FEATURE: Add analysis.
        /// <summary>
        ///     Gets or sets the flag indicating whether to enable analysis.
        /// </summary>
        public bool EnableAnalysis { get; set; }

        /// <summary>
        ///     Gets or sets the mapping orientation.
        /// </summary>
        public MappingOrientation MappingOrientation { get; set; }
    }
}
