using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AdminBarbeariaUserRepositoryTests : IDisposable
{
    private readonly BarbAppDbContext _context;
    private readonly IAdminBarbeariaUserRepository _repository;

    public AdminBarbeariaUserRepositoryTests()
    {
        var mockTenantContext = new Mock<ITenantContext>();
        mockTenantContext.Setup(tc => tc.IsAdminCentral).Returns(true);
        mockTenantContext.Setup(tc => tc.BarbeariaId).Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BarbAppDbContext(options, mockTenantContext.Object);
        _repository = new AdminBarbeariaUserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "admin@barbearia.com";
        var barbeariaId = Guid.NewGuid();
        var code = UniqueCode.Create("ABCDEFGH");
        var barbershop = Barbershop.Create("Barbearia Test", code);
        barbershop.GetType().GetProperty("Id")?.SetValue(barbershop, barbeariaId);

        var user = AdminBarbeariaUser.Create(barbeariaId, email, "hashedpassword", "Admin Barbearia");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAndBarbeariaIdAsync(email, barbeariaId);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@barbearia.com";
        var barbeariaId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByEmailAndBarbeariaIdAsync(email, barbeariaId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToContext()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var user = AdminBarbeariaUser.Create(barbeariaId, "newadmin@barbearia.com", "hashedpassword", "New Admin Barbearia");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().Be(user);
        var savedUser = await _context.AdminBarbeariaUsers.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser.Should().BeEquivalentTo(user);
    }
}