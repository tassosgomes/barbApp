// BarbApp.Infrastructure.Tests/Repositories/CustomerRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var telefone = "11999999999";
        var customer = Customer.Create(barbeariaId, telefone, "João Customer");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result!.Telefone.Should().Be(telefone);
        result.BarbeariaId.Should().Be(barbeariaId);
        result.Name.Should().Be("João Customer");
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var telefone = "11999999999";

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenCustomerExistsInDifferentBarbearia_ReturnsNull()
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
        var telefone = "11999999999";
        var customer = Customer.Create(barbeariaId1, telefone, "João Customer");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsCustomerToDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("DEF67892");
        var barbearia = CreateTestBarbershop("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var customer = Customer.Create(barbeariaId, "11999999999", "Novo Customer");

        // Act
        var result = await _repository.AddAsync(customer);

        // Assert
        result.Should().Be(customer);
        _context.Customers.Should().Contain(customer);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}