using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Exceptions;

/// <summary>
///     Represents an error that occurs when a given value of a command options is invalidated.
/// </summary>
internal class InvalidOptionException(CommandOption option) : Exception(CreateMessage(option))
{
    private static string CreateMessage(CommandOption option)
    {
        return string.Format(Resources.InvalidOptionException, option.Value() ?? "N/A", option.ValueName);
    }
}
