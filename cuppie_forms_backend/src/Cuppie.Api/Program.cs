using Serilog;
using Cuppie.Api.Extensions;


    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = LoggingConfiguration.CreateLogger();
    
    builder.Configuration.AddEnvironmentVariables();
    builder.Host.UseSerilog();
    builder.Services.AddSingleton(Log.Logger);

    builder.Services.AddPresentation(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UsePresentation(app.Environment);

    Log.Information("Приложение запущено");
    app.Run();