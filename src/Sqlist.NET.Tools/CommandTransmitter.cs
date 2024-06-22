using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Exceptions;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools;
internal class CommandTransmitter(IProcessManager processRunner) : ICommandTransmitter
{
    public Task TransmitAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : TransmittableCommandHandler
    {
        if (handler.Project is null)
            throw new CommandTransmissionException(Resources.HandlerProjectNullException);

        if (string.IsNullOrEmpty(handler.Project.Value()))
            throw new CommandTransmissionException(Resources.HandlerProjectNoValueException);

        var exec = "dotnet";
        var args = new List<string> { "run", "--" };

        AddArguments(args,
            handler.Project,
            handler.Framework,
            handler.Configuration,
            handler.Runtime,
            handler.LaunchProfile,
            handler.Force,
            handler.NoBuild,
            handler.NoRestore);

        using var process = processRunner.Prepare(exec, args);
        return process.RunAsync(cancellationToken);
    }

    public static void AddArguments(in List<string> args, params CommandOption?[] options)
    {
        foreach (var option in options)
        {
            if (option is null) continue;

            foreach (var value in option.Values)
            {
                args.Add(GetOptionName(option));

                if (!string.IsNullOrEmpty(value))
                    args.Add(value);
            }
        }
    }

    public static string GetOptionName(CommandOption option)
    {
        if (option.LongName is not null)
            return "--" + option.LongName;

        return "-" + (option.ShortName ?? option.SymbolName ?? string.Empty);
    }
}
