using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration.Extensions
{
    internal static class AssemblyExtensions
    {
        public static void ReadEmbeddedResources(this Assembly assembly, string path, Action<string, string?> action)
        {
            assembly.ReadEmbeddedResources(path, (resource, stream) =>
            {
                action(resource, stream);
                return Task.CompletedTask;
            }).Wait();
        }

        public static async Task ReadEmbeddedResources(this Assembly assembly, string path, Func<string, string?, Task> asyncAction)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            var basePath = assembly.GetName().Name + "." + path.Trim('.', ' ');
            var resNames = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(basePath))
                .OrderBy(name => name);

            foreach (var name in resNames)
            {
                using var stream = assembly.GetManifestResourceStream(name);
                using var reader = new StreamReader(stream!);

                await asyncAction(name, await reader.ReadToEndAsync());
            }
        }
    }
}
