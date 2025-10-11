// BarbApp.Infrastructure.Tests/Repositories/AdminBarbeariaUserRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AdminBarbeariaUserRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly AdminBarbeariaUserRepository _repository;
    private readonly Mock<ITenantContext> _tenantContextMock;

    public AdminBarbeariaUserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new AdminBarbeariaUserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var barbeariaCode = BarbeariaCode.Create("ABC23456");
        var barbearia = Barbershop.Create("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var email = "admin@barbearia.com";
        var user = AdminBarbeariaUser.Create(barbeariaId, email, "hashedpassword", "Admin Barbearia");
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAndBarbeariaIdAsync(email, barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.BarbeariaId.Should().Be(barbeariaId);
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var email = "nonexistent@barbearia.com";

        // Act
        var result = await _repository.GetByEmailAndBarbeariaIdAsync(email, barbeariaId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_WhenUserExistsInDifferentBarbearia_ReturnsNull()
    {
        // Arrange
        var barbeariaCode1 = BarbeariaCode.Create("ABC23456");
        var barbearia1 = Barbershop.Create("Barbearia 1", barbeariaCode1);
        var barbeariaCode2 = BarbeariaCode.Create("XYZ98765");
        var barbearia2 = Barbershop.Create("Barbearia 2", barbeariaCode2);
        await _context.Barbershops.AddRangeAsync(barbearia1, barbearia2);
        await _context.SaveChangesAsync();

        var barbeariaId1 = barbearia1.Id;
        var barbeariaId2 = barbearia2.Id;
        var email = "admin@barbearia.com";
        var user = AdminBarbeariaUser.Create(barbeariaId1, email, "hashedpassword", "Admin Barbearia");
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAndBarbeariaIdAsync(email, barbeariaId2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsUserToDatabase()
    {
        // Arrange
        var barbeariaCode = BarbeariaCode.Create("DEF67892");
        var barbearia = Barbershop.Create("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var user = AdminBarbeariaUser.Create(barbeariaId, "newadmin@barbearia.com", "hashedpassword", "New Admin Barbearia");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().Be(user);
        _context.AdminBarbeariaUsers.Should().Contain(user);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}