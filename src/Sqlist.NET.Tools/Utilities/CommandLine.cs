namespace Sqlist.NET.Tools.Utilities;
internal static class CommandLine
{
    public static string[] Args
    {
        get;
#if TEST
        set;
#endif
    } = Environment.GetCommandLineArgs().Skip(1).ToArray();

    public static string String => string.Join(' ', Args);
}
