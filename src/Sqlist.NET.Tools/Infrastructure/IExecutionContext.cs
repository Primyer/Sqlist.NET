using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Infrastructure;
internal interface IExecutionContext
{
    bool IsToolContext { get; set; }

    string[] CommandLineArgs { get; }

    CommandLineApplication Application { get; }

    /// <exception cref="InvalidOperationException" />
    CommandLineApplication SelectedCommand { get; }
}
