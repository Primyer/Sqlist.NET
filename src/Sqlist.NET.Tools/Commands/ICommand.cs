using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Commands;
internal interface ICommand
{
    bool Configured { get; }
    void Configure(CommandLineApplication app);
}
