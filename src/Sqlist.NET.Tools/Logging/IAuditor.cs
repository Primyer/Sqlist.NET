namespace Sqlist.NET.Tools.Logging;
internal interface IAuditor
{
    static bool IsVerbose { get; }
    static bool NoColor { get; }
    static bool PrefixOutput { get; }

    void WriteError(string? message);
    void WriteError(Exception ex, string? message = null);
    void WriteWarning(string? message);
    void WriteInformation(string? message);
    void WriteData(string? message);
    void WriteVerbose(string? message);
}
