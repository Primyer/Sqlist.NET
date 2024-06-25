using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Commands;
internal interface ICommand
{
    void Configure(CommandLineApplication app);
}
