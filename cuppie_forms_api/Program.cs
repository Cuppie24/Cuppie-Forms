using cuppie_forms_api.Data;
using cuppie_forms_api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddSingleton(Log.Logger);
    builder.Host.UseSerilog();
    // Add services to the container.
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<JwtTokenService>();
    builder.Services.AddCors(options =>
    
    {
        options.AddPolicy("FrontDev", corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .WithOrigins(
                    "http://localhost:5174",
                    "http://localhost:3000",
                    "http://localhost:80",
                    "http://localhost:5173"
                    )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // важно для передачи cookies
        });
    });
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("cuppieContext"); 
    builder.Services.AddDbContext<CuppieDbContext>(options =>
        options.UseNpgsql(connectionString
                          ?? throw new InvalidOperationException("Connection string 'cuppieContext' not found.")));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapOpenApi();
    }

    app.UseCors("FrontDev");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    
    // Log information about app startup
    Log.Information("Приложение запущено на порту: {Port}", app.Urls);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ошибка при запуске приложения");
}
finally
{
    Log.CloseAndFlush();
}