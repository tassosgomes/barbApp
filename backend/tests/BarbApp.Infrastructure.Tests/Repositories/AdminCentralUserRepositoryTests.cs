// BarbApp.Infrastructure.Tests/Repositories/AdminCentralUserRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AdminCentralUserRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly AdminCentralUserRepository _repository;

    public AdminCentralUserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new AdminCentralUserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var email = "admin@example.com";
        var user = AdminCentralUser.Create(email, "hashedpassword", "Admin User");
        await _context.AdminCentralUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsUserToDatabase()
    {
        // Arrange
        var user = AdminCentralUser.Create("newadmin@example.com", "hashedpassword", "New Admin");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().Be(user);
        _context.AdminCentralUsers.Should().Contain(user);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}