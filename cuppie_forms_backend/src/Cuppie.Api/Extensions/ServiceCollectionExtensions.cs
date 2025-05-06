using System.Text;
using Cuppie.Application.Interfaces;
using Cuppie.Infrastructure.Data;
using Cuppie.Infrastructure.Services;
using Cuppie.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            options.AddPolicy("FrontDev", cors =>
            {
                cors.WithOrigins(
                        "http://localhost:5174",
                        "http://localhost:3000",
                        "http://localhost:80",
                        "http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.Configure<JwtOptions>(config.GetSection("JWT"));
        
        services.AddSingleton<TokenValidationParameters>(sp =>
        {
            var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        });
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