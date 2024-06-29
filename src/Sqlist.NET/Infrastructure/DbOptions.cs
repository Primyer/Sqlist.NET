using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Infrastructure;

public class DbOptions : IDbOptions
{
    public string? ConnectionString { get; set; }
    public Version? DbVersion { get; set; }
    public bool EnableSensitiveLogging { get; set; }
    public bool EnableAnalysis { get; set; }
    public MappingOrientation MappingOrientation { get; set; }
    public Encloser? DelimitedEncloser { get; set; }
}
