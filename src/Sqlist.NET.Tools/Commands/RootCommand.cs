using McMaster.Extensions.CommandLineUtils;

using System.Reflection;

namespace Sqlist.NET.Tools.Commands;
internal class RootCommand(MigrationCommand migrationCommand) : CommandBase
{
    public override void Configure(CommandLineApplication app)
    {
        if (Configured) return;

        app.Command("migrate", migrationCommand.Configure);
        app.VersionOption("-v|--version", GetVersion);

        base.Configure(app);
    }

    private static string GetVersion()
        => typeof(RootCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
}
