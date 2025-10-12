// BarbApp.Infrastructure.Tests/Repositories/AddressRepositoryTests.cs
using System.Linq;
using System.Threading;
using BarbApp.Domain.Entities;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AddressRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly AddressRepository _repository;

    public AddressRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new AddressRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAddressExists_ReturnsAddress()
    {
        // Arrange
        var address = CreateAddress();
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(address.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ZipCode.Should().Be(address.ZipCode);
        result.Street.Should().Be(address.Street);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAddressDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_TracksAddressWithoutSavingChanges()
    {
        // Arrange
        var address = CreateAddress();

        // Act
        await _repository.AddAsync(address, CancellationToken.None);

        // Assert
        var entry = _context.ChangeTracker.Entries<Address>().Single();
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().Be(address);
        _context.Addresses.Local.Should().Contain(address);
    }

    private static Address CreateAddress()
    {
        return Address.Create(
            zipCode: "01310100",
            street: "Av. Paulista",
            number: "1000",
            complement: "Conjunto 10",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
