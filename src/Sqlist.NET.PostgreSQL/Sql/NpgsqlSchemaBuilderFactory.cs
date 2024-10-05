using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;

namespace Sqlist.NET.Sql;

/// <summary>
///     Initializes a new instance of the <see cref="NpgsqlSchemaBuilderFactory"/> class.
/// </summary>
/// <param name="options">The Sqlist configuration options.</param>
internal class NpgsqlSchemaBuilderFactory(IOptions<DbOptions> options) : ISchemaBuilderFactory
{
    public ISchemaBuilder Create()
    {
        var encloser = options.Value.DelimitedEnclosure ?? new DummyEnclosure();
        return new NpgsqlSchemaBuilder(encloser);
    }
}
