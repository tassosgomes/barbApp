using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class BarberRepositoryTests : IDisposable
{
    private readonly BarbAppDbContext _context;
    private readonly IBarberRepository _repository;

    public BarberRepositoryTests()
    {
        var mockTenantContext = new Mock<ITenantContext>();
        mockTenantContext.Setup(tc => tc.IsAdminCentral).Returns(true);
        mockTenantContext.Setup(tc => tc.BarbeariaId).Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BarbAppDbContext(options, mockTenantContext.Object);
        _repository = new BarberRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_ShouldReturnBarber_WhenBarberExists()
    {
        // Arrange
        var telefone = "11999999999";
        var barbeariaId = Guid.NewGuid();
        var code = UniqueCode.Create("ABCDEFGH");
        var barbershop = Barbershop.Create("Barbearia Test", code);
        barbershop.GetType().GetProperty("Id")?.SetValue(barbershop, barbeariaId);

        var barber = Barber.Create(barbeariaId, telefone, "João Barber");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.Barbers.AddAsync(barber);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().BeEquivalentTo(barber);
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_ShouldReturnNull_WhenBarberDoesNotExist()
    {
        // Arrange
        var telefone = "11999999999";
        var barbeariaId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarbeariaIdAsync_ShouldReturnBarbers_WhenBarbersExist()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var code = UniqueCode.Create("ABCDEFGH");
        var barbershop = Barbershop.Create("Barbearia Test", code);
        barbershop.GetType().GetProperty("Id")?.SetValue(barbershop, barbeariaId);

        var barbers = new List<Barber>
        {
            Barber.Create(barbeariaId, "11999999991", "João"),
            Barber.Create(barbeariaId, "11999999992", "Maria")
        };

        await _context.Barbershops.AddAsync(barbershop);
        await _context.Barbers.AddRangeAsync(barbers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarbeariaIdAsync(barbeariaId);

        // Assert
        result.Should().BeEquivalentTo(barbers);
    }

    [Fact]
    public async Task GetByBarbeariaIdAsync_ShouldReturnEmptyList_WhenNoBarbersExist()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByBarbeariaIdAsync(barbeariaId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddBarberToContext()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barber = Barber.Create(barbeariaId, "11999999993", "Novo Barber");

        // Act
        var result = await _repository.AddAsync(barber);

        // Assert
        result.Should().Be(barber);
        var savedBarber = await _context.Barbers.FindAsync(barber.Id);
        savedBarber.Should().NotBeNull();
        savedBarber.Should().BeEquivalentTo(barber);
    }
}