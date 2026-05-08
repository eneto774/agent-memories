using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace AgentService.Api;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNamingPolicy = System
                    .Text
                    .Json
                    .JsonNamingPolicy
                    .CamelCase;
                opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddOpenApi();
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        services.AddEndpointsApiExplorer();
    }

    public static void UseWebServices(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var feature =
                    context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (feature?.Error != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(feature.Error, "Unhandled exception");
                    await context.Response.WriteAsJsonAsync(
                        new { error = "An unexpected error occurred." }
                    );
                }
            });
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
