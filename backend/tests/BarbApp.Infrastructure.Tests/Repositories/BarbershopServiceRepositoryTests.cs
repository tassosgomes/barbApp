// BarbApp.Infrastructure.Tests/Repositories/BarbershopServiceRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class BarbershopServiceRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly BarbershopServiceRepository _repository;

    public BarbershopServiceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new BarbershopServiceRepository(_context);
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task GetByIdAsync_WhenServiceExists_ReturnsService()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var service = BarbershopService.Create(barbeariaId, "Corte de Cabelo", "Corte masculino completo", 30, 25.00m);
        await _context.BarbershopServices.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(service.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Corte de Cabelo");
        result.BarbeariaId.Should().Be(barbeariaId);
        result.DurationMinutes.Should().Be(30);
        result.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenServiceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(serviceId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ReturnsAllServicesForBarbearia()
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
        var services = new[]
        {
            BarbershopService.Create(barbeariaId1, "Corte", "Corte masculino", 30, 25.00m),
            BarbershopService.Create(barbeariaId1, "Barba", "Aparar barba", 15, 15.00m),
            BarbershopService.Create(barbeariaId2, "Sobrancelha", "Design de sobrancelha", 10, 10.00m)
        };
        await _context.BarbershopServices.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync(barbeariaId1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Corte");
        result.Should().Contain(s => s.Name == "Barba");
        result.Should().NotContain(s => s.Name == "Sobrancelha");
    }

    [Fact]
    public async Task ListAsync_WhenIsActiveFilterIsTrue_ReturnsOnlyActiveServices()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var activeService = BarbershopService.Create(barbeariaId, "Corte Ativo", "Corte ativo", 30, 25.00m);
        var inactiveService = BarbershopService.Create(barbeariaId, "Corte Inativo", "Corte inativo", 30, 20.00m);
        inactiveService.Deactivate();

        await _context.BarbershopServices.AddRangeAsync(activeService, inactiveService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync(barbeariaId, isActive: true);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(s => s.Name == "Corte Ativo");
        result.Should().NotContain(s => s.Name == "Corte Inativo");
    }

    [Fact]
    public async Task ListAsync_WhenIsActiveFilterIsFalse_ReturnsOnlyInactiveServices()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var activeService = BarbershopService.Create(barbeariaId, "Corte Ativo", "Corte ativo", 30, 25.00m);
        var inactiveService = BarbershopService.Create(barbeariaId, "Corte Inativo", "Corte inativo", 30, 20.00m);
        inactiveService.Deactivate();

        await _context.BarbershopServices.AddRangeAsync(activeService, inactiveService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync(barbeariaId, isActive: false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(s => s.Name == "Corte Inativo");
        result.Should().NotContain(s => s.Name == "Corte Ativo");
    }

    [Fact]
    public async Task ListAsync_WhenNoServices_ReturnsEmptyList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var result = await _repository.ListAsync(barbeariaId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InsertAsync_AddsServiceToDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("DEF67892");
        var barbearia = CreateTestBarbershop("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var service = BarbershopService.Create(barbeariaId, "Novo Serviço", "Descrição do serviço", 45, 35.00m);

        // Act
        await _repository.InsertAsync(service);
        await _context.SaveChangesAsync();

        // Assert
        var savedService = await _context.BarbershopServices.FindAsync(service.Id);
        savedService.Should().NotBeNull();
        savedService!.Name.Should().Be("Novo Serviço");
        savedService.DurationMinutes.Should().Be(45);
        savedService.Price.Should().Be(35.00m);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesServiceInDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("DEF67892");
        var barbearia = CreateTestBarbershop("Barbearia Nova", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);
        await _context.SaveChangesAsync();

        var barbeariaId = barbearia.Id;
        var service = BarbershopService.Create(barbeariaId, "Serviço Original", "Descrição original", 30, 25.00m);
        await _context.BarbershopServices.AddAsync(service);
        await _context.SaveChangesAsync();

        // Modify service
        service.Update("Serviço Atualizado", "Descrição atualizada", 45, 35.00m);

        // Act
        await _repository.UpdateAsync(service);
        await _context.SaveChangesAsync();

        // Assert
        var updatedService = await _context.BarbershopServices.FindAsync(service.Id);
        updatedService.Should().NotBeNull();
        updatedService!.Name.Should().Be("Serviço Atualizado");
        updatedService.Description.Should().Be("Descrição atualizada");
        updatedService.DurationMinutes.Should().Be(45);
        updatedService.Price.Should().Be(35.00m);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}