using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Commands;

namespace Sqlist.NET.Tools.Infrastructure;
internal class EmbeddedAppExecutor : IApplicationExecutor, IDisposable
{
    private readonly CommandLineApplication _app = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbeddedAppExecutor"/> class.
    /// </summary>
    public EmbeddedAppExecutor(MigrationCommand migrationCommand)
    {
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
}
