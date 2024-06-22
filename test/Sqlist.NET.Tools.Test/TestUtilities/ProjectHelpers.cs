using System.Reflection;

namespace Sqlist.NET.Tools.Tests.TestUtilities;
internal static class ProjectHelpers
{
    public static string GetSandboxProjectPath()
    {
        var projectDir = GetProjectDirectory();
        var sandboxDir = Path.Combine(projectDir, "../Sqlist.NET.Tools.Sandbox");

        return Path.GetFullPath(sandboxDir);
    }

    public static string GetProjectDirectory()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return Path.GetDirectoryName(assembly.Location) ?? string.Empty;
    }
}
