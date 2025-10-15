// BarbApp.Infrastructure.Tests/Repositories/BarberRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class BarberRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly BarberRepository _repository;

    public BarberRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new BarberRepository(_context);
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenBarberExists_ReturnsBarber()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var telefone = "11999999999";
        var barber = Barber.Create(barbeariaId, "João Barber", "joao@test.com", "hashedpassword", telefone);
        await _context.Barbers.AddAsync(barber);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().NotBeNull();
        result!.Phone.Should().Be(telefone);
        result.BarbeariaId.Should().Be(barbeariaId);
        result.Name.Should().Be("João Barber");
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenBarberDoesNotExist_ReturnsNull()
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
    public async Task GetByTelefoneAndBarbeariaIdAsync_WhenBarberExistsInDifferentBarbearia_ReturnsNull()
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
        var barber = Barber.Create(barbeariaId1, "João Barber", "joao@test.com", "hashedpassword", telefone);
        await _context.Barbers.AddAsync(barber);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarbeariaIdAsync_ReturnsAllBarbersForBarbearia()
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
        var barbers = new[]
        {
            Barber.Create(barbeariaId1, "João", "joao@test.com", "hashedpassword1", "11999999991"),
            Barber.Create(barbeariaId1, "Maria", "maria@test.com", "hashedpassword2", "11999999992"),
            Barber.Create(barbeariaId2, "Pedro", "pedro@test.com", "hashedpassword3", "11999999993")
        };
        await _context.Barbers.AddRangeAsync(barbers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarbeariaIdAsync(barbeariaId1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.Name == "João");
        result.Should().Contain(b => b.Name == "Maria");
        result.Should().NotContain(b => b.Name == "Pedro");
    }

    [Fact]
    public async Task GetByBarbeariaIdAsync_WhenNoBarbers_ReturnsEmptyList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByBarbeariaIdAsync(barbeariaId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_AddsBarberToDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("DEF67892");
        var barbearia = CreateTestBarbershop("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var barber = Barber.Create(barbeariaId, "Novo Barber", "novo@test.com", "hashedpassword", "11999999999");

        // Act
        await _repository.InsertAsync(barber);
        await _context.SaveChangesAsync();

        // Assert
        var savedBarber = await _context.Barbers.FindAsync(barber.Id);
        savedBarber.Should().NotBeNull();
        savedBarber!.Name.Should().Be("Novo Barber");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}