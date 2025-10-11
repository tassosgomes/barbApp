using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly BarbAppDbContext _context;
    private readonly ICustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var mockTenantContext = new Mock<ITenantContext>();
        mockTenantContext.Setup(tc => tc.IsAdminCentral).Returns(true);
        mockTenantContext.Setup(tc => tc.BarbeariaId).Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BarbAppDbContext(options, mockTenantContext.Object);
        _repository = new CustomerRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var telefone = "11999999999";
        var barbeariaId = Guid.NewGuid();
        var code = BarbeariaCode.Create("ABCDEFGH");
        var barbershop = Barbershop.Create("Barbearia Test", code);
        barbershop.GetType().GetProperty("Id")?.SetValue(barbershop, barbeariaId);

        var customer = Customer.Create(barbeariaId, telefone, "João Customer");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelefoneAndBarbeariaIdAsync(telefone, barbeariaId);

        // Assert
        result.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetByTelefoneAndBarbeariaIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
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
    public async Task AddAsync_ShouldAddCustomerToContext()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var customer = Customer.Create(barbeariaId, "11999999993", "Novo Customer");

        // Act
        var result = await _repository.AddAsync(customer);

        // Assert
        result.Should().Be(customer);
        var savedCustomer = await _context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer.Should().BeEquivalentTo(customer);
    }
}