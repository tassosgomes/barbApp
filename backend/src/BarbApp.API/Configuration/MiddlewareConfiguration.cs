using BarbApp.API.Filters;
using BarbApp.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BarbApp.API.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Global exception handling (first)
        app.UseGlobalExceptionHandler();

        // Swagger (development only)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarbApp API v1");
                c.RoutePrefix = string.Empty; // Swagger at root
            });
        }

        // CORS
        app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors");

        // Response Compression
        app.UseResponseCompression();

        // Output Cache
        app.UseOutputCache();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Rate Limiting
        app.UseRateLimiter();

        // Prometheus metrics
        // app.UseHttpMetrics();
        // app.UseMetricServer();

        // Tenant middleware (after authentication)
        app.UseTenantMiddleware();
        app.UseSentryScopeEnrichment();

        // Controllers
        app.MapControllers();

        // Health checks
        app.MapHealthChecks("/health");

        // Test endpoints for middleware testing
        ConfigureTestEndpoints(app);

        return app;
    }

    private static void ConfigureTestEndpoints(WebApplication app)
    {
        app.MapGet("/test/unauthorized", () => { throw new BarbApp.Domain.Exceptions.UnauthorizedException("Test unauthorized"); });
        app.MapGet("/test/forbidden", () => { throw new BarbApp.Domain.Exceptions.ForbiddenException("Test forbidden"); });
        app.MapGet("/test/notfound", () => { throw new BarbApp.Domain.Exceptions.NotFoundException("Test not found"); });
        app.MapGet("/test/validation", () => { throw new FluentValidation.ValidationException("Test validation"); });
        app.MapGet("/test/unhandled", () => { throw new Exception("Test unhandled"); });
        app.MapGet("/test/tenant-context", () => "Tenant context test");

        // Weather forecast endpoint for authentication testing
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .RequireAuthorization()
        .WithName("GetWeatherForecast")
        .WithOpenApi();
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}