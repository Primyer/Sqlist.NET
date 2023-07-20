namespace Sqlist.NET.Migration.Tests.Utilities;
public static class AssemblyUtility
{
    public static string GetEmbeddedResource(string name)
    {
        using var stream = typeof(AssemblyUtility).Assembly.GetManifestResourceStream(name);
        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }
}
