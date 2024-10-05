using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Infrastructure;

/// <summary>
///     Provides the configuration options needed for a regular Sqlist API.
/// </summary>
internal interface IDbOptions
{
    /// <summary>
    ///     Gets or sets the connection string to the target database.
    /// </summary>
    string? ConnectionString { get; }

    /// <summary>
    ///     Gets or sets the version of the database.
    /// </summary>
    Version? DbVersion { get; }

    /// <summary>
    ///     Gets or sets the flag indicating whether to log sensitive information such as command parameters.
    /// </summary>
    bool EnableSensitiveLogging { get; }

    // FEATURE: Add analysis.
    /// <summary>
    ///     Gets or sets the flag indicating whether to enable analysis.
    /// </summary>
    bool EnableAnalysis { get; }

    /// <summary>
    ///     Gets or sets the mapping orientation.
    /// </summary>
    MappingOrientation MappingOrientation { get; }

    /// <summary>
    ///     Gets or sets the delimited encloser for case-sensitive naming in SQL queries.
    /// </summary>
    Enclosure? DelimitedEnclosure { get; }
}
