using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class LandingPageServiceRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly LandingPageServiceRepository _repository;

    public LandingPageServiceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new LandingPageServiceRepository(_context);
    }

    [Fact]
    public async Task GetByLandingPageIdAsync_ReturnsServicesOrderedByDisplayOrder()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Teste", "ABCD2345", "12345678000190");
        var service1 = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var service2 = CreateService(barbershop.Id, "Barba", 20, 25.00m);
        var service3 = CreateService(barbershop.Id, "Combo", 50, 55.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511999999999");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddRangeAsync(service1, service2, service3);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService1 = LandingPageService.Create(config.Id, service1.Id, 3, true);
        var landingService2 = LandingPageService.Create(config.Id, service2.Id, 1, true);
        var landingService3 = LandingPageService.Create(config.Id, service3.Id, 2, true);

        await _context.LandingPageServices.AddRangeAsync(landingService1, landingService2, landingService3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByLandingPageIdAsync(config.Id, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Service.Name.Should().Be("Barba");
        result[0].DisplayOrder.Should().Be(1);
        result[1].Service.Name.Should().Be("Combo");
        result[1].DisplayOrder.Should().Be(2);
        result[2].Service.Name.Should().Be("Corte");
        result[2].DisplayOrder.Should().Be(3);
    }

    [Fact]
    public async Task GetByLandingPageIdAsync_WhenNoServices_ReturnsEmptyList()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Vazia", "EMTY2345", "12345678000191");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511988888888");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByLandingPageIdAsync(config.Id, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByLandingPageIdAsync_IncludesServiceDetails()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Detalhes", "DTAL2345", "12345678000192");
        var service = CreateService(barbershop.Id, "Corte Premium", 45, 50.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511977777777");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);
        await _context.LandingPageServices.AddAsync(landingService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByLandingPageIdAsync(config.Id, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Service.Should().NotBeNull();
        result[0].Service.Name.Should().Be("Corte Premium");
        result[0].Service.DurationMinutes.Should().Be(45);
        result[0].Service.Price.Should().Be(50.00m);
    }

    [Fact]
    public async Task DeleteByLandingPageIdAsync_RemovesAllServicesForLandingPage()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Delete", "DELT2345", "12345678000193");
        var service1 = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var service2 = CreateService(barbershop.Id, "Barba", 20, 25.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511966666666");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddRangeAsync(service1, service2);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService1 = LandingPageService.Create(config.Id, service1.Id, 1, true);
        var landingService2 = LandingPageService.Create(config.Id, service2.Id, 2, true);
        await _context.LandingPageServices.AddRangeAsync(landingService1, landingService2);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByLandingPageIdAsync(config.Id, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var remainingServices = await _context.LandingPageServices
            .Where(lps => lps.LandingPageConfigId == config.Id)
            .ToListAsync();
        remainingServices.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteByLandingPageIdAsync_WhenNoServices_DoesNotThrowException()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Sem Serviços", "NSRV2345", "12345678000194");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511955555555");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
        {
            await _repository.DeleteByLandingPageIdAsync(config.Id, CancellationToken.None);
            await _context.SaveChangesAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExistsAsync_WhenRelationExists_ReturnsTrue()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Existe", "EXST5678", "12345678000195");
        var service = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511944444444");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);
        await _context.LandingPageServices.AddAsync(landingService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(config.Id, service.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenRelationDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Não Existe", "NOTEXIST", "12345678000196");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511933333333");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(config.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InsertAsync_AddsServiceToContext()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Insert", "NSRT2345", "12345678000197");
        var service = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511922222222");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);

        // Act
        await _repository.InsertAsync(landingService, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var savedService = await _context.LandingPageServices.FindAsync(landingService.Id);
        savedService.Should().NotBeNull();
        savedService!.ServiceId.Should().Be(service.Id);
    }

    [Fact]
    public async Task InsertRangeAsync_AddsMultipleServicesToContext()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Range", "RNGE2345", "12345678000198");
        var service1 = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var service2 = CreateService(barbershop.Id, "Barba", 20, 25.00m);
        var service3 = CreateService(barbershop.Id, "Combo", 50, 55.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511911111111");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddRangeAsync(service1, service2, service3);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingServices = new[]
        {
            LandingPageService.Create(config.Id, service1.Id, 1, true),
            LandingPageService.Create(config.Id, service2.Id, 2, true),
            LandingPageService.Create(config.Id, service3.Id, 3, false)
        };

        // Act
        await _repository.InsertRangeAsync(landingServices, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var savedServices = await _context.LandingPageServices
            .Where(lps => lps.LandingPageConfigId == config.Id)
            .ToListAsync();
        savedServices.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingService()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Update", "PDAT2345", "12345678000199");
        var service = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511900000000");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);
        await _context.LandingPageServices.AddAsync(landingService);
        await _context.SaveChangesAsync();

        // Act
        landingService.UpdateDisplayOrder(5);
        landingService.Hide();
        await _repository.UpdateAsync(landingService, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var updatedService = await _context.LandingPageServices.FindAsync(landingService.Id);
        updatedService.Should().NotBeNull();
        updatedService!.DisplayOrder.Should().Be(5);
        updatedService.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesServiceFromContext()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Delete Single", "DLSN2345", "12345678000200");
        var service = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511899999999");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);
        await _context.LandingPageServices.AddAsync(landingService);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(landingService, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var deletedService = await _context.LandingPageServices.FindAsync(landingService.Id);
        deletedService.Should().BeNull();
    }

    private Barbershop CreateBarbershop(string name, string code, string document)
    {
        var address = Address.Create(
            "12345678",
            "Rua Teste",
            "123",
            null,
            "Centro",
            "São Paulo",
            "SP");

        return Barbershop.Create(
            name,
            Document.Create(document),
            "11999999999",
            "Owner Test",
            "test@test.com",
            address,
            UniqueCode.Create(code),
            "test-user");
    }

    private BarbershopService CreateService(Guid barbeariaId, string name, int duration, decimal price)
    {
        return BarbershopService.Create(barbeariaId, name, null, duration, price);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
