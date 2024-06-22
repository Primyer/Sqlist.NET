
using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Commands;

using System.Reflection;

namespace Sqlist.NET.Tools.Infrastructure;
internal class ToolCliExecutor : IApplicationExecutor, IDisposable
{
    private readonly CommandLineApplication _app = new() { Name = "dotnet sqlist" };

/// <summary>
///     Initializes a new instance of the <see cref="ToolCliExecutor"/> class.
/// </summary>
public ToolCliExecutor(MigrationCommand migrationCommand, IExecutionContext context)
    {
        context.IsTransmitter = true;

        _app.VersionOption("-v|--version", GetVersion);
        migrationCommand.Configure(_app);
    }

    public void Dispose()
    {
        ((IDisposable)_app).Dispose();
    }

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        return _app.ExecuteAsync(args, cancellationToken);
    }

    private static string GetVersion()
        => typeof(ToolCliExecutor).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
}
