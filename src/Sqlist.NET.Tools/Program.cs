using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Logging;

try
{
    await Host.CreateDefaultBuilder(args)
        .UseCommandLineApplication()
        .RunConsoleAsync();
}
catch (Exception ex)
{
    if (ex is CommandParsingException)
        Reporter.WriteVerbose(ex.ToString());
    else
        Reporter.WriteInformation(ex.ToString());

    Reporter.WriteError(ex.Message);
}