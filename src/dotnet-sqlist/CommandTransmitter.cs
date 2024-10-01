using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Exceptions;
using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

using System.Text.RegularExpressions;

namespace Sqlist.NET.Tools;
internal partial class CommandTransmitter(IProcessManager processRunner, IExecutionContext context, IAuditor auditor) : ICommandTransmitter
{
    public Task TransmitAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : TransmittableCommandHandler
    {
        if (handler.Project is null)
            throw new CommandTransmissionException(Resources.ProjectOptionIsNull);

        const string exec = "dotnet";
        var args = new List<string> { "run" };
        var opts = handler.GetOptions().Except([handler.Project]);

        AddProjectOption(args, handler.Project);
        AddArguments(args, opts);

        args.AddRange(["--", .. WhiteSpaceRegex().Split(Resources.RootCommandName)]);
        AddTransmittableArgs(args, handler, context.SelectedCommand);

        return processRunner
            .Prepare(exec, args)
            .RunAsync(cancellationToken);
    }

    public static void AddTransmittableArgs<THandler>(List<string> args, THandler handler, CommandLineApplication selectedCommand) where THandler : TransmittableCommandHandler
    {
        var command = selectedCommand;
        var options = selectedCommand.Options.Except(handler.GetOptions());
        var cmdName = new List<string>();

        while (command.Parent is not null)
        {
            cmdName.Add(command.Name!);
            command = command.Parent;
        }

        cmdName.Reverse();
        args.AddRange(cmdName);
        
        AddArguments(args, options);
    }

    public static void AddArguments(in List<string> args, IEnumerable<CommandOption?> options)
    {
        foreach (var option in options)
        {
            if (option is null) continue;

            foreach (var value in option.Values)
            {
                args.Add(option.GetOptionName());

                if (!string.IsNullOrEmpty(value))
                    args.Add(value);
            }
        }
    }

    private void AddProjectOption(in List<string> args, CommandOption project)
    {
        var directory = !project.HasValue()
            ? Directory.GetCurrentDirectory()
            : project.Value();

        var fileName = GetProjectFileName(directory!);

        args.Add(project.GetOptionName());
        args.Add(directory!);

        auditor.WriteInformation(string.Format(Resources.RunningProject, fileName));
    }

    private static string GetProjectFileName(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        
        const string ext = ".csproj";

        if (Directory.Exists(path))
        {
            var csprojFileName = Directory.GetFiles(path, "*" + ext, SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (csprojFileName is null)
            {
                throw new CommandTransmissionException(Resources.InvalidProjectDirectory);
            }
            return csprojFileName;
        }
        else if (File.Exists(path) && path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }
        
        throw new CommandTransmissionException(Resources.DirectoryPathNotFound);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex WhiteSpaceRegex();
}
