using cuppie.Components;
using cuppie.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using cuppie.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CuppieDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("cuppieContext") ?? throw new InvalidOperationException("Connection string 'cuppieContext' not found.")));


builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddControllers(); 
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<AuthHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Если токен уже задан (например, из заголовка Authorization) — ничего не делаем
                if (string.IsNullOrEmpty(context.Token))
                {
                    // Пробуем взять из cookie
                    if (context.Request.Cookies.ContainsKey(AuthHandler.JwtCookieName))
                    {
                        context.Token = context.Request.Cookies[AuthHandler.JwtCookieName];
                    }
                }

                return Task.CompletedTask;
            }
        };
        var jwtSettings = builder.Configuration.GetSection(AuthHandler.JwtSectionName);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings[AuthHandler.JwtIssuerName],
            ValidateAudience = true,
            ValidAudience = jwtSettings[AuthHandler.JwtAudienceName],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings[AuthHandler.JwtKeyName])),
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000/") });
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:5005", "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Критически важно для передачи cookies
    });
});





builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    //app.UseHsts();
    app.UseMigrationsEndPoint();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseCors("LocalPolicy");


app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
