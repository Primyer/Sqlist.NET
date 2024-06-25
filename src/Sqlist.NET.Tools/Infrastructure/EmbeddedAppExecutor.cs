using Sqlist.NET.Tools.Commands;

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
        return _context.Application.ExecuteAsync(args, cancellationToken);
    }
}
