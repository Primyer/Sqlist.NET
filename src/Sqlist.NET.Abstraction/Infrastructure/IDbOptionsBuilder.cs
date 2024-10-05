using Microsoft.Extensions.Configuration;

using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Infrastructure;
public interface IDbOptionsBuilder
{
    /// <summary>
    ///     Sets the connection string for the target database.
    /// </summary>
    /// <param name="connectionString">The configuration representing the database connection string.</param>
    void SetConnectionString(IConfiguration connectionString);

    /// <summary>
    ///     Sets the version that represents the target database.
    ///     <para>
    ///         The database version represents the starting point of migration for the compiled binary.
    ///     </para>
    /// </summary>
    /// <param name="version">The database version.</param>
    void SetDbVersion(Version version);

    /// <summary>
    ///     Enables sensitive information logging such as command parameters.
    /// </summary>
    void EnableSensitiveLogging();

    /// <summary>
    ///     Enables analysis options.
    /// </summary>
    void EnableAnalysis();

    /// <summary>
    ///     Sets the mapping orientation.
    /// </summary>
    /// <param name="orientation">The mapping orientation to set.</param>
    void SetMappingOrientation(MappingOrientation orientation);

    /// <summary>
    ///     Sets a custom delimited encloser for case-sensitive naming in SQL queries.
    /// </summary>
    /// <param name="customEncloser">The custom <see cref="Enclosure"/> implementation to be used.</param>
    void WithCaseSensitiveNaming(Enclosure? customEncloser = null);

    /// <summary>
    ///     Sets the default naming in SQL queries to case-insensitive.
    /// </summary>
    void WithCaseInsensitiveNaming();
}