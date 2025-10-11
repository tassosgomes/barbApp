using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AdminCentralUserRepositoryTests : IDisposable
{
    private readonly BarbAppDbContext _context;
    private readonly IAdminCentralUserRepository _repository;

    public AdminCentralUserRepositoryTests()
    {
        var mockTenantContext = new Mock<ITenantContext>();
        mockTenantContext.Setup(tc => tc.IsAdminCentral).Returns(true);
        mockTenantContext.Setup(tc => tc.BarbeariaId).Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BarbAppDbContext(options, mockTenantContext.Object);
        _repository = new AdminCentralUserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "admin@example.com";
        var user = AdminCentralUser.Create(email, "hashedpassword", "Admin User");
        await _context.AdminCentralUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToContext()
    {
        // Arrange
        var user = AdminCentralUser.Create("newadmin@example.com", "hashedpassword", "New Admin");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().Be(user);
        var savedUser = await _context.AdminCentralUsers.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser.Should().BeEquivalentTo(user);
    }
}