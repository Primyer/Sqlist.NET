namespace Sqlist.NET.Tools.Infrastructure;
internal interface IExecutionContext
{
    bool IsTransmitter { get; set; }
    string[] CommandLineArgs { get; }
}
