namespace Sqlist.NET.Tools.Logging;
internal interface IAuditor
{
    void WriteError(string? message);
    void WriteError(Exception ex, string? message = null);
    void WriteWarning(string? message);
    void WriteInformation(string? message);
    void WriteData(string? message);
    void WriteVerbose(string? message);
}
