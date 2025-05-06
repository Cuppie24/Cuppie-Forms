using Serilog;
namespace Cuppie.Api.Extensions;

public static class LoggingConfiguration
{
    public static Serilog.ILogger CreateLogger() =>
        new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .CreateLogger();
}