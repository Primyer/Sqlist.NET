using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Exceptions;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools;
internal class CommandTransmitter(IProcessRunner processRunner) : ICommandTransmitter
{
    public Task TransmitAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : TransmittableCommandHandler
    {
        if (handler.Project is null)
            throw new CommandTransmissionException(Resources.HandlerProjectNullException);

        if (!handler.Project.HasValue())
            throw new CommandTransmissionException(Resources.HandlerProjectNoValueException);

        var exec = "dotnet";
        var args = new List<string> { "run" };

        AddArguments(args,
            handler.Project,
            handler.Framework,
            handler.Configuration,
            handler.Runtime,
            handler.LaunchProfile,
            handler.Force,
            handler.NoBuild,
            handler.NoRestore);

        return processRunner.RunAsync(exec, args, cancellationToken: cancellationToken);
    }

    private static void AddArguments(in List<string> args, params CommandOption?[] options)
    {
        foreach (var option in options)
        {
            if (option is null) continue;

            foreach (var value in option.Values)
            {
                args.Add(option.LongName ?? option.ShortName ?? option.SymbolName!);

                if (value is not null)
                    args.Add(value);
            }
        }
    }
}
