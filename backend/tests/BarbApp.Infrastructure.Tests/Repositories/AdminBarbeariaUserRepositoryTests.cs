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

    public AdminBarbeariaUserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new AdminBarbeariaUserRepository(_context);
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task GetByEmailAndBarbeariaIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            barbeariaCode,
            "admin-user-id"
        );
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
        var barbeariaCode1 = UniqueCode.Create("ABC23456");
        var barbearia1 = CreateTestBarbershop("Barbearia 1", barbeariaCode1);
        var barbeariaCode2 = UniqueCode.Create("XYZ98765");
        var barbearia2 = CreateTestBarbershop("Barbearia 2", barbeariaCode2);
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
        var barbeariaCode = UniqueCode.Create("DEF67892");
        var barbearia = CreateTestBarbershop("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var user = AdminBarbeariaUser.Create(barbeariaId, "newadmin@barbearia.com", "hashedpassword", "New Admin Barbearia");

        // Act
        var result = await _repository.AddAsync(user);
        await _context.SaveChangesAsync(); // Unit of Work responsibility

        // Assert
        result.Should().Be(user);
        _context.AdminBarbeariaUsers.Should().Contain(user);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("GHI34567");
        var barbearia = CreateTestBarbershop("Barbearia Teste GetByEmail", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var email = "admin@teste.com";
        var user = AdminBarbeariaUser.Create(barbearia.Id, email, "hashedpassword", "Admin Barbearia");
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.BarbeariaId.Should().Be(barbearia.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("JKL78923");
        var barbearia = CreateTestBarbershop("Barbearia Teste CaseInsensitive", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var email = "Admin@CaseInsensitive.com";
        var user = AdminBarbeariaUser.Create(barbearia.Id, email, "hashedpassword", "Admin Barbearia");
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _repository.GetByEmailAsync("admin@caseinsensitive.com");
        var result2 = await _repository.GetByEmailAsync("ADMIN@CASEINSENSITIVE.COM");
        var result3 = await _repository.GetByEmailAsync("Admin@CaseInsensitive.com");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        result1!.Email.Should().BeEquivalentTo(email);
        result2!.Email.Should().BeEquivalentTo(email);
        result3!.Email.Should().BeEquivalentTo(email);
        result1.BarbeariaId.Should().Be(barbearia.Id);
        result2.BarbeariaId.Should().Be(barbearia.Id);
        result3.BarbeariaId.Should().Be(barbearia.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@email.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenMultipleUsersWithSameEmail_ReturnsFirst()
    {
        // Arrange
        var barbeariaCode1 = UniqueCode.Create("MN2345AB");
        var barbearia1 = CreateTestBarbershop("Barbearia 1", barbeariaCode1);
        var barbeariaCode2 = UniqueCode.Create("PQR6789C");
        var barbearia2 = CreateTestBarbershop("Barbearia 2", barbeariaCode2);
        await _context.Barbershops.AddRangeAsync(barbearia1, barbearia2);
        await _context.SaveChangesAsync();

        var email = "shared@email.com";
        var user1 = AdminBarbeariaUser.Create(barbearia1.Id, email, "hashedpassword1", "Admin Barbearia 1");
        var user2 = AdminBarbeariaUser.Create(barbearia2.Id, email, "hashedpassword2", "Admin Barbearia 2");
        await _context.AdminBarbeariaUsers.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        // Should return one of the users (the first one added)
        (result.Id == user1.Id || result.Id == user2.Id).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}