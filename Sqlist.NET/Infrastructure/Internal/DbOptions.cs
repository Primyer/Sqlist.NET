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
    }
}
