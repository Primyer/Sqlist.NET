using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Infrastructure;
public interface IExecutionContext
{
    bool IsToolContext { get; set; }

    CommandLineApplication Application { get; }

    /// <exception cref="InvalidOperationException" />
    CommandLineApplication SelectedCommand { get; }
}
