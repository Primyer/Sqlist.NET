using Sqlist.NET.Tools.Out;

using System.Diagnostics.CodeAnalysis;

using static Sqlist.NET.Tools.Out.AnsiConstants;

namespace Sqlist.NET.Tools.Logging;
internal class Auditor : IAuditor
{
    public const string ErrorPrefix = "[fail]: ";
    public const string WarningPrefix = "[warn]: ";
    public const string InfoPrefix = "[info]: ";
    public const string DebugPrefix = "[dbug]: ";
    public const string TracePrefix = "[trce]: ";

    public static bool IsVerbose { get; set; }
    public static bool NoColor { get; set; }
    public static bool PrefixOutput { get; set; }

    [return: NotNullIfNotNull(nameof(value))]
    public static string? Colorize(string? value, Func<string?, string> colorizeFunc)
        => NoColor ? value : colorizeFunc(value);

    public void WriteError(string? message)
        => WriteLine(Prefix(ErrorPrefix, Colorize(message, x => Bold + Red + x + Reset)));

    public void WriteError(Exception ex, string? message = null)
    {
        message = string.Join(Environment.NewLine, message, ex.ToString());
        WriteError(message);
    }

    public void WriteWarning(string? message)
        => WriteLine(Prefix(WarningPrefix, Colorize(message, x => Bold + Yellow + x + Reset)));

    public void WriteInformation(string? message)
        => WriteLine(Prefix(InfoPrefix, message));

    public void WriteDebug(string? message)
        => WriteLine(Prefix(DebugPrefix, Colorize(message, x => Bold + Gray + x + Reset)));

    public void WriteTrace(string? message)
    {
        if (IsVerbose)
        {
            WriteLine(Prefix(TracePrefix, Colorize(message, x => Bold + Black + x + Reset)));
        }
    }

    private static string? Prefix(string prefix, string? value)
    {
        if (!PrefixOutput)
            return value;

        if (value == null)
            return prefix;

        return prefix + value.Replace(Environment.NewLine, Environment.NewLine + prefix);
    }

    public virtual void WriteLine(string? message)
    {
        if (NoColor)
            Console.WriteLine(message);
        else
            AnsiConsole.WriteLine(message);
    }
}
