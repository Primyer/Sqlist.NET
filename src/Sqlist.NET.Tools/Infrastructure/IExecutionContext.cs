using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Infrastructure;
internal interface IExecutionContext
{
    bool IsToolContext { get; set; }

    CommandLineApplication Application { get; }

    /// <exception cref="InvalidOperationException" />
    CommandLineApplication SelectedCommand { get; }
}
