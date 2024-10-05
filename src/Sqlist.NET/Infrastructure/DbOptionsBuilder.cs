using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;
using Sqlist.NET.Utilities;

namespace Sqlist.NET.Infrastructure;

/// <summary>
///     Provides the API to configure <see cref="DbOptions"/>.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DbOptionsBuilder"/> class.
/// </remarks>
public class DbOptionsBuilder(DbOptions options)
{
    public bool CaseSensitiveNaming { get; private set; }

    /// <summary>
    ///     Sets the connection string for the target database.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    public void SetConnectionString(string connectionString)
    {
        Check.NotNull(connectionString, nameof(connectionString));
        options.ConnectionString = connectionString;
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

        options.DbVersion = version;
    }

    /// <summary>
    ///     Enables sensitive information logging such as command parameters.
    /// </summary>
    public void EnableSensitiveLogging()
    {
        options.EnableSensitiveLogging = true;
    }

    /// <summary>
    ///     Enables analysis options.
    /// </summary>
    public void EnableAnalysis()
    {
        options.EnableAnalysis = true;
    }

    /// <summary>
    ///     Sets the mapping orientation.
    /// </summary>
    /// <param name="orientation">The mapping orientation to set.</param>
    public void SetMappingOrientation(MappingOrientation orientation)
    {
        options.MappingOrientation = orientation;
    }

    /// <summary>
    ///     Sets a custom delimited enclosure for case-sensitive naming in SQL queries.
    /// </summary>
    /// <param name="customEnclosure">The custom <see cref="Enclosure"/> implementation to be used.</param>
    public void WithCaseSensitiveNaming(Enclosure? customEnclosure = null)
    {
        Check.NotNull(customEnclosure, nameof(customEnclosure));

        CaseSensitiveNaming = true;
        options.DelimitedEnclosure = customEnclosure;
    }

    /// <summary>
    ///     Sets the default naming in SQL queries to case-insensitive.
    /// </summary>
    public void WithCaseInsensitiveNaming()
    {
        CaseSensitiveNaming = false;
        options.DelimitedEnclosure = null;
    }

    public DbOptions GetOptions() => options;
}