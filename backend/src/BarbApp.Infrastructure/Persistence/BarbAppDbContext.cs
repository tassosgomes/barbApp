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
    public DbSet<Customer> Customers => Set<Customer>();

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

        modelBuilder.Entity<Customer>().HasQueryFilter(c =>
            _tenantContext.IsAdminCentral || c.BarbeariaId == _tenantContext.BarbeariaId);
    }
}