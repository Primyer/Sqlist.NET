using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Infrastructure;

/// <summary>
///     Provides the configuration options needed for a regular Sqlist API.
/// </summary>
public class DbOptions
{
    /// <summary>
    ///     Gets or sets the connection string to the target database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    ///     Gets or sets the version of the database.
    /// </summary>
    public Version? DbVersion { get; set; }

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

    /// <summary>
    ///     Gets or sets the delimited encloser for case-sensitive naming in SQL queries.
    /// </summary>
    public Encloser? DelimitedEncloser { get; set; }
}
