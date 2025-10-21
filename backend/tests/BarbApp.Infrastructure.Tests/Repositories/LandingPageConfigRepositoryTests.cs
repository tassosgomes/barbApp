using System.Linq;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class LandingPageConfigRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly LandingPageConfigRepository _repository;

    public LandingPageConfigRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new LandingPageConfigRepository(_context);
    }

    [Fact]
    public async Task GetByBarbershopIdAsync_WhenConfigExists_ReturnsConfigWithRelations()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Teste", "ABCD2345", "12345678000190");
        var service = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var config = LandingPageConfig.Create(
            barbershop.Id,
            1,
            "+5511999999999",
            aboutText: "Barbearia teste");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddAsync(service);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService = LandingPageService.Create(config.Id, service.Id, 1, true);
        await _context.LandingPageServices.AddAsync(landingService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarbershopIdAsync(barbershop.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.BarbershopId.Should().Be(barbershop.Id);
        result.Barbershop.Should().NotBeNull();
        result.Services.Should().HaveCount(1);
        result.Services.First().Service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByBarbershopIdAsync_WhenConfigDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByBarbershopIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarbershopCodeAsync_WhenConfigExists_ReturnsConfig()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Teste", "EFGH5678", "12345678000191");
        var config = LandingPageConfig.Create(barbershop.Id, 2, "+5511988888888");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarbershopCodeAsync("EFGH5678", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.BarbershopId.Should().Be(barbershop.Id);
        result.Barbershop.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByBarbershopCodeAsync_WhenCodeDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByBarbershopCodeAsync("NONEXIST", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarbershopIdWithServicesAsync_OnlyReturnsVisibleServices()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Teste", "MNPQ9999", "12345678000192");
        var service1 = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var service2 = CreateService(barbershop.Id, "Barba", 20, 25.00m);
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511977777777");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddRangeAsync(service1, service2);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService1 = LandingPageService.Create(config.Id, service1.Id, 1, true);
        var landingService2 = LandingPageService.Create(config.Id, service2.Id, 2, false);
        await _context.LandingPageServices.AddRangeAsync(landingService1, landingService2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarbershopIdWithServicesAsync(barbershop.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Services.Should().HaveCount(2); // Returns all services (both visible and invisible)
        result.Services.Count(s => s.IsVisible).Should().Be(1); // But only 1 is visible
    }

    [Fact]
    public async Task GetPublicByCodeAsync_OnlyReturnsPublishedConfigs()
    {
        // Arrange
        var barbershop1 = CreateBarbershop("Barbearia Publicada", "PQRS2345", "12345678000193");
        var barbershop2 = CreateBarbershop("Barbearia Não Publicada", "NPQR5678", "12345678000194");
        
        var publishedConfig = LandingPageConfig.Create(barbershop1.Id, 1, "+5511966666666");
        var unpublishedConfig = LandingPageConfig.Create(barbershop2.Id, 1, "+5511955555555");
        unpublishedConfig.Unpublish();
        
        await _context.Barbershops.AddRangeAsync(barbershop1, barbershop2);
        await _context.LandingPageConfigs.AddRangeAsync(publishedConfig, unpublishedConfig);
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _repository.GetPublicByCodeAsync("PQRS2345", CancellationToken.None);
        var result2 = await _repository.GetPublicByCodeAsync("NPQR5678", CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result1!.IsPublished.Should().BeTrue();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task GetPublicByCodeAsync_ReturnsVisibleServicesInOrder()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Teste", "MNPQ2345", "12345678000195");
        var service1 = CreateService(barbershop.Id, "Corte", 30, 35.00m);
        var service2 = CreateService(barbershop.Id, "Barba", 20, 25.00m);
        var service3 = CreateService(barbershop.Id, "Combo", 50, 55.00m);
        
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511944444444");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.BarbershopServices.AddRangeAsync(service1, service2, service3);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        var landingService1 = LandingPageService.Create(config.Id, service1.Id, 3, true);
        var landingService2 = LandingPageService.Create(config.Id, service2.Id, 1, true);
        var landingService3 = LandingPageService.Create(config.Id, service3.Id, 2, false); // Not visible
        
        await _context.LandingPageServices.AddRangeAsync(landingService1, landingService2, landingService3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPublicByCodeAsync("MNPQ2345", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Services.Should().HaveCount(3); // Returns all services
        result.Services.Count(s => s.IsVisible).Should().Be(2); // But only 2 are visible
        // Order is maintained by DisplayOrder in the services collection
        var visibleServices = result.Services.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder).ToList();
        visibleServices.Select(s => s.Service.Name).Should().ContainInOrder("Barba", "Corte");
    }

    [Fact]
    public async Task ExistsForBarbershopAsync_WhenConfigExists_ReturnsTrue()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Existe", "EXST2345", "12345678000196");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511933333333");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsForBarbershopAsync(barbershop.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsForBarbershopAsync_WhenConfigDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsForBarbershopAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InsertAsync_AddsConfigToContext()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Insert", "NSRT2345", "12345678000197");
        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        var config = LandingPageConfig.Create(barbershop.Id, 3, "+5511922222222");

        // Act
        await _repository.InsertAsync(config, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var savedConfig = await _context.LandingPageConfigs.FindAsync(config.Id);
        savedConfig.Should().NotBeNull();
        savedConfig!.TemplateId.Should().Be(3);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingConfig()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Update", "PDAT2345", "12345678000198");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511911111111");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        config.Update(templateId: 4, aboutText: "Updated about text");
        await _repository.UpdateAsync(config, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var updatedConfig = await _context.LandingPageConfigs.FindAsync(config.Id);
        updatedConfig.Should().NotBeNull();
        updatedConfig!.TemplateId.Should().Be(4);
        updatedConfig.AboutText.Should().Be("Updated about text");
    }

    [Fact]
    public async Task DeleteAsync_RemovesConfigFromContext()
    {
        // Arrange
        var barbershop = CreateBarbershop("Barbearia Delete", "DELT2345", "12345678000199");
        var config = LandingPageConfig.Create(barbershop.Id, 1, "+5511900000000");
        
        await _context.Barbershops.AddAsync(barbershop);
        await _context.LandingPageConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(config, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var deletedConfig = await _context.LandingPageConfigs.FindAsync(config.Id);
        deletedConfig.Should().BeNull();
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
