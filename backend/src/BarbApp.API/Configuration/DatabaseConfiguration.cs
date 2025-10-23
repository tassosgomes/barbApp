using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BarbApp.API.Configuration;

public static class DatabaseConfiguration
{
    public static async Task RunDatabaseMigrations(WebApplication app)
    {
        // Database migration (Development - Relational DB only)
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Only migrate if using a relational database provider
            if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migration completed");
            }
            else
            {
                logger.LogInformation("Using in-memory database - skipping migration");
            }
        }
    }
}