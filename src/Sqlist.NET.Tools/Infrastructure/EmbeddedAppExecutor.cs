using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Infrastructure;

internal class EmbeddedAppExecutor : IApplicationExecutor, IDisposable
{
    private readonly IExecutionContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbeddedAppExecutor"/> class.
    /// </summary>
    public EmbeddedAppExecutor(
        IExecutionContext context,
#if DEBUG
        TestCommand testCommand,
#endif
        MigrationCommand migrationCommand)
    {
        _context = context;

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
        var arguments = string.Join(' ', args);
        if (!arguments.StartsWith(Resources.RootCommandName))
        {
            throw new InvalidOperationException($"The arguments must start with '{Resources.RootCommandName}'.");
        }

        return _context.Application.ExecuteAsync(
            args.Skip(2).ToArray(),
            cancellationToken);
    }
}
