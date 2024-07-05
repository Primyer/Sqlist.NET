using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Properties;

using System.Reflection;

namespace Sqlist.NET.Tools;
internal class ToolCliExecutor : IApplicationExecutor, IDisposable
{
    private readonly IExecutionContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolCliExecutor"/> class.
    /// </summary>
    public ToolCliExecutor(
    IExecutionContext context,
#if DEBUG
    TestCommand testCommand,
#endif
    MigrationCommand migrationCommand)
    {
        _context = context;
        _context.IsToolContext = true;

        _context.Application.Name = Resources.RootCommandName;
        _context.Application.VersionOption("-v|--version", GetVersion);

        migrationCommand.Configure(_context.Application);
#if DEBUG
        testCommand.Configure(_context.Application);
#endif
    }

    public void Dispose()
    {
        ((IDisposable)_context.Application).Dispose();
    }

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        return _context.Application.ExecuteAsync(args, cancellationToken);
    }

    private static string GetVersion()
        => typeof(ToolCliExecutor).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
}
