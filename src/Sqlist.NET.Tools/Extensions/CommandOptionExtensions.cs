using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Extensions;
public static class CommandOptionExtensions
{
    public static string GetOptionName(this CommandOption option)
    {
        if (option.LongName is not null)
            return "--" + option.LongName;

        return "-" + (option.ShortName ?? option.SymbolName ?? string.Empty);
    }
}
