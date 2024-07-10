using System;
using System.Reflection;

namespace Sqlist.NET.Migration.Infrastructure;
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
    /// <param name="path">The path to the migration scripts. The default path is <b>Migration.Scripts</b>.</param>
    /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
    public MigrationOptionsBuilder SetScriptsAssembly(Assembly assembly, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        _options.ScriptsAssembly = assembly;

        if (path is not null)
            return SetScriptsPath(path);

        return this;
    }

    /// <summary>
    ///     Sets the path to the migration scripts. The default path is <b>Migration.Scripts</b>.
    /// </summary>
    /// <param name="path">The path to the migration scripts.</param>
    /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
    public MigrationOptionsBuilder SetScriptsPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _options.ScriptsPath = path;
        return this;
    }

    /// <summary>
    ///     Sets the <paramref name="assembly"/> of where the migration roadmap exist.
    /// </summary>
    /// <param name="assembly">The assembly of where the migration roadmap exists.</param>
    /// <param name="path">The path to the migration roadmap. The default path is <b>Migration.Roadmap</b>.</param>
    /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
    public MigrationOptionsBuilder SetRoadmapAssembly(Assembly assembly, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        _options.RoadmapAssembly = assembly;

        if (path is not null)
            return SetRoadmapPath(path);

        return this;
    }

    /// <summary>
    ///     Sets the path to the migration roadmap. The default path is <b>Migration.Roadmap</b>.
    /// </summary>
    /// <param name="path">The path to the migration roadmap.</param>
    /// <returns>The current instance of the <see cref="MigrationOptionsBuilder"/>.</returns>
    public MigrationOptionsBuilder SetRoadmapPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

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