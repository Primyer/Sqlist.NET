﻿using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Handlers;
public abstract class TransmittableCommandHandler : ICommandHandler
{
    public CommandOption? Project { get; private set; }
    public CommandOption? Framework { get; private set; }
    public CommandOption? Configuration { get; private set; }
    public CommandOption? Runtime { get; private set; }
    public CommandOption? LaunchProfile { get; private set; }
    public CommandOption? Force { get; private set; }
    public CommandOption? NoBuild { get; private set; }
    public CommandOption? NoRestore { get; private set; }

    internal void Initialize(CommandLineApplication command)
    {
        Project = command.Option("--project <PROJECT>", Resources.ProjectDescription, CommandOptionType.SingleValue);
        Framework = command.Option("-f|--framework <FRAMEWORK>", Resources.FrameworkDescription, CommandOptionType.SingleValue);
        Configuration = command.Option("-c|--configuration <CONFIGURATION>", Resources.ConfigurationDescription, CommandOptionType.SingleValue);
        Runtime = command.Option("-r|--runtime <RUNTIME_IDENTIFIER>", Resources.RuntimeDescription, CommandOptionType.SingleValue);
        LaunchProfile = command.Option("--launch-profile <NAME>", Resources.LaunchProfileDescription, CommandOptionType.SingleOrNoValue);
        Force = command.Option("--force", Resources.ForceDescription, CommandOptionType.NoValue);
        NoBuild = command.Option("--no-build", Resources.NoBuildDescription, CommandOptionType.NoValue);
        NoRestore = command.Option("--no-restore", Resources.NoRestoreDescription, CommandOptionType.NoValue);
    }

    public abstract Task<int> OnExecuteAsync(CancellationToken cancellationToken);
    public CommandOption?[] GetOptions()
    {
        return [
            Project,
            Framework,
            Configuration,
            Runtime,
            LaunchProfile,
            Force,
            NoBuild,
            NoRestore
        ];
    }
}
