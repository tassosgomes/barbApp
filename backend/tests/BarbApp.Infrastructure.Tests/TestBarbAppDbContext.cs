// BarbApp.Infrastructure.Tests/TestBarbAppDbContext.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests;

public class TestBarbAppDbContext : BarbAppDbContext
{
    public TestBarbAppDbContext(DbContextOptions<BarbAppDbContext> options)
        : base(options, new TestTenantContext())
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply query filters like in production, but with test tenant context
    }

    private class TestTenantContext : ITenantContext
    {
        public Guid? BarbeariaId => Guid.Empty;
        public string? UniqueCode => null;
        public bool IsAdminCentral => true;
        public string UserId => "test-user";
        public string Role => "admin";

        public void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode)
        {
            // No-op for tests
        }

        public void Clear()
        {
            // No-op for tests
        }
    }
}