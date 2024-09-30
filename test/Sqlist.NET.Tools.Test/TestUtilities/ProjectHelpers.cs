namespace Sqlist.NET.Tools.Tests.TestUtilities;
internal static class ProjectHelpers
{
    public static string GetSandboxProjectPath()
    {
        var projectDir = GetProjectDirectory();
        var sandboxDir = Path.Combine(projectDir, "../Sqlist.NET.Tools.Sandbox");

        return Path.GetFullPath(sandboxDir);
    }

    private static string GetProjectDirectory()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(baseDirectory);

        while (directoryInfo is not null && directoryInfo.GetFiles("*.csproj").Length == 0)
        {
            directoryInfo = directoryInfo.Parent;
        }

        return directoryInfo?.FullName ?? string.Empty;
    }
}
