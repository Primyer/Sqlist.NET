using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sqlist.NET.Migration.Extensions;

using EmbeddedResource = Tuple<string, string?>;

internal static class AssemblyExtensions
{
    public static async IAsyncEnumerable<EmbeddedResource> GetEmbeddedResourcesAsync(this Assembly assembly, string path)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var resourceNames = GetResourceNames(assembly, path).ToList();
        foreach (var name in resourceNames)
        {
            await using var stream = assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream!);

            yield return new EmbeddedResource(name, await reader.ReadToEndAsync());
        }
    }

    private static IEnumerable<string> GetResourceNames(Assembly assembly, string path)
    {
        var basePath = assembly.GetName().Name + "." + path.Trim('.', ' ');
        
        return assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(basePath))
            .OrderBy(name => name);
    }
}