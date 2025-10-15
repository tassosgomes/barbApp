// BarbApp.Infrastructure/Persistence/BarbAppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BarbApp.Infrastructure.Persistence;

public class BarbAppDbContextFactory : IDesignTimeDbContextFactory<BarbAppDbContext>
{
    public BarbAppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BarbApp.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BarbAppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseNpgsql(connectionString);

        // For design-time, create a dummy tenant context
        var tenantContext = new DesignTimeTenantContext();

        return new BarbAppDbContext(optionsBuilder.Options, tenantContext);
    }
}

public class DesignTimeTenantContext : Domain.Interfaces.ITenantContext
{
    public Guid? BarbeariaId => Guid.Empty; // Dummy value
    public string? UniqueCode => null;
    public bool IsAdminCentral => true; // Allow all for design-time
    public string UserId => "design-time";
    public string Role => "AdminCentral";

    public void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode) { }
    public void Clear() { }
}