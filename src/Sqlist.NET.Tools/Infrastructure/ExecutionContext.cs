using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Infrastructure;

/// <summary>
///     Initializes a new instance of the <see cref="ExecutionContext"/> class.
/// </summary>
internal class ExecutionContext : IExecutionContext
{
    private bool? _isToolContext;
    private CommandLineApplication? _selectedCommand;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExecutionContext"/> class.
    /// </summary>
    public ExecutionContext()
    {
        Application.OnParsingComplete(result => _selectedCommand = result.SelectedCommand);
    }

    public bool IsToolContext
    {
        get => _isToolContext ?? false;
        set
        {
            if (!_isToolContext.HasValue)
                _isToolContext = value;
        }
    }

    public CommandLineApplication Application { get; } = new();

    public CommandLineApplication SelectedCommand
    {
        get
        {
            if (_selectedCommand is null)
                throw new InvalidOperationException(Resources.NoSelectedCommandException);

            return _selectedCommand;
        }
    }
}
