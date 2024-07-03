namespace Sqlist.NET.Tools.Out;
internal static class AnsiConsole
{
    public static readonly AnsiTextWriter Out = new(Console.Out);

    public static void WriteLine(string? text)
        => Out.WriteLine(text);
}