using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Logging;

var host = Host.CreateDefaultBuilder(args)
    .UseCommandLineApplication()
    .UseConsoleLifetime()
    .Build();

var auditor = host.Services.GetRequiredService<IAuditor>();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    if (ex is CommandParsingException)
        auditor.WriteVerbose(ex.ToString());
    else
        auditor.WriteInformation(ex.ToString());

    auditor.WriteError(ex.Message);
}