using System;
using System.Reflection;

namespace Sqlist.NET.Migration.Infrastructure
{
    public class MigrationOptionsBuilder
    {
        private readonly MigrationOptions _options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationOptionsBuilder"/> class.
        /// </summary>
        internal MigrationOptionsBuilder() : this(new())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationOptionsBuilder"/> class.
        /// </summary>
        internal MigrationOptionsBuilder(MigrationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
        }

        /// <summary>
        ///     Sets the <paramref name="assembly"/> of where the migration scripts exist.
        /// </summary>
        /// <param name="assembly">The assembly of where the migration scripts exists.</param>
        /// <param name="path">The path to the migration scripts. The default path is <u><b>Scripts</b></u>.</param>
        /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
        public MigrationOptionsBuilder SetMigrationAssembly(Assembly assembly, string? path = null)
        {
            _options.ScriptsAssembly = assembly;

            if (path != null)
                _options.ScriptsPath = path;

            return this;
        }

        /// <summary>
        ///     Sets the <paramref name="assembly"/> of where the data migration roadmap exit.
        /// </summary>
        /// <param name="assembly">The assembly of where the data migration roadmap exit.</param>
        /// <param name="path">The path to the migration  roadmap phases. The default path is <u><b>Roadmap</b></u>.</param>
        /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
        public MigrationOptionsBuilder SetDataMigrationRoadmapAssembly(Assembly assembly, string? path = null)
        {
            _options.RoadmapAssembly = assembly;

            if (path != null)
                _options.RoadmapPath = path;

            return this;
        }

        /// <summary>
        ///     Sets the name of the table where migration operations are to be recorded.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="schema">The schema path of where the migration table is found or to be created.</param>
        /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
        public MigrationOptionsBuilder SetMigrationTable(string table, string? schema = null)
        {
            _options.SchemaTable = table;
            _options.SchemaTableSchema = schema;

            return this;
        }

        public MigrationOptions GetOptions() => _options;
    }
}
