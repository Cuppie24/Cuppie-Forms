using Microsoft.AspNetCore.HttpOverrides;

namespace Cuppie.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UsePresentation(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseForwardedHeaders();

        // CORS must be before authentication and authorization
        app.UseCors("FrontDev");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}