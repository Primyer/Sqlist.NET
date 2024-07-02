using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Out;

using System.Diagnostics.CodeAnalysis;
using System.Text;

using static Sqlist.NET.Tools.Out.AnsiConstants;

namespace Sqlist.NET.Tools.Logging;

/// <summary>
///     Initializes a new instance of the <see cref="Auditor"/> class.
/// </summary>
internal class Auditor(IExecutionContext context) : IAuditor
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
    {
        message = Prefix(ErrorPrefix, message);
        message = Colorize(message, x => Bold + Red + x + Reset);

        WriteLine(message);
    }

    public void WriteError(Exception ex, string? message = null)
    {
        WriteError(message);
        WriteError(IsVerbose ? ex.ToString() : ex.Message);
    }

    public void WriteWarning(string? message)
    {
        message = Prefix(WarningPrefix, message);
        message = Colorize(message, x => Bold + Yellow + x + Reset);

        WriteLine(message);
    }

    public void WriteInformation(string? message)
        => WriteLine(Prefix(InfoPrefix, message));

    public void WriteDebug(string? message)
    {
        message = Prefix(DebugPrefix, message);
        message = Colorize(message, x => Bold + Gray + x + Reset);

        WriteLine(message);
    }

    public void WriteTrace(string? message)
    {
        if (!IsVerbose) return;

        message = Prefix(TracePrefix, message);
        message = Colorize(message, x => Bold + Black + x + Reset);
        
        WriteLine(message);
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
        if (NoColor || !context.IsToolContext)
            Console.WriteLine(message);
        else
            AnsiConsole.WriteLine(message);
    }
}
