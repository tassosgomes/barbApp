// BarbApp.Infrastructure/Persistence/BarbAppDbContext.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence;

public class BarbAppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public BarbAppDbContext(
        DbContextOptions<BarbAppDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Barbershop> Barbershops => Set<Barbershop>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<AdminCentralUser> AdminCentralUsers => Set<AdminCentralUser>();
    public DbSet<AdminBarbeariaUser> AdminBarbeariaUsers => Set<AdminBarbeariaUser>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<BarbershopService> BarbershopServices => Set<BarbershopService>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<LandingPageConfig> LandingPageConfigs => Set<LandingPageConfig>();
    public DbSet<LandingPageService> LandingPageServices => Set<LandingPageService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarbAppDbContext).Assembly);

        // Global Query Filters para isolamento multi-tenant
        modelBuilder.Entity<AdminBarbeariaUser>().HasQueryFilter(a =>
            _tenantContext.IsAdminCentral || a.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<Barber>().HasQueryFilter(b =>
            _tenantContext.IsAdminCentral || b.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<BarbershopService>().HasQueryFilter(s =>
            _tenantContext.IsAdminCentral || s.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<Customer>().HasQueryFilter(c =>
            _tenantContext.IsAdminCentral || c.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<Appointment>().HasQueryFilter(a =>
            _tenantContext.IsAdminCentral || a.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<LandingPageConfig>().HasQueryFilter(l =>
            _tenantContext.IsAdminCentral || l.BarbershopId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<LandingPageService>().HasQueryFilter(l =>
            _tenantContext.IsAdminCentral || l.LandingPageConfig.BarbershopId == _tenantContext.BarbeariaId);
    }
}