using System.Net;
using System.Text;
using Cuppie.Application.Interfaces.DAO;
using Cuppie.Application.Interfaces.Services;
using Cuppie.Infrastructure.DAO;
using Cuppie.Infrastructure.Data;
using Cuppie.Infrastructure.Services;
using Cuppie.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cuppie.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration config)
    {
        
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddOpenApi();
        services.AddCors(options =>
        {
            var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? 
                             config.GetValue<string>("CORS_ALLOWED_ORIGINS") ?? 
                             "https://cuppie.cup";
            
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .ToArray();
            
            // Log CORS configuration for debugging
            Console.WriteLine($"CORS Origins configured: {string.Join(", ", origins)}");
            
            options.AddPolicy("FrontDev", cors =>
            {
                cors.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
            });
        });
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                var jwtOptions = config.GetSection("JWT").Get<JwtOptions>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateIssuerSigningKey = true
                };
                // Чтобы JWT брался из cookie (НЕ стандартное поведение, нужно вручную)
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("jwt", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
               
            });
        services.AddAuthorization();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
            options.KnownProxies.Add(IPAddress.Parse("127.18.0.3")); 
            options.ForwardLimit = 1;
        });

        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICryptoService, CryptoService>();
        services.AddScoped<IUserDao, UserDao>();
        services.AddScoped<IRefreshTokenDao, RefreshTokenDao>();
        services.Configure<JwtOptions>(config.GetSection("JWT"));
        
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection") 
                               ?? config.GetConnectionString("cuppieContext");

        if (connectionString == null)
            throw new InvalidOperationException("Connection string not found.");

        services.AddDbContext<CuppieDbContext>(options =>
            options.UseNpgsql(connectionString));

        // регистрируешь репозитории, sender-ы и прочее
        // services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}