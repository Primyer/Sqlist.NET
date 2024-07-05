using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Commands;
public interface ICommand
{
    void Configure(CommandLineApplication app);
}
