using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class LandingPageServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IBarbershopServiceRepository> _barbershopServiceRepositoryMock;
    private readonly Mock<ILandingPageConfigRepository> _landingPageConfigRepositoryMock;
    private readonly Mock<ILandingPageServiceRepository> _landingPageServiceRepositoryMock;
    private readonly Mock<ILogger<Application.UseCases.LandingPageService>> _loggerMock;
    private readonly Application.UseCases.LandingPageService _service;

    public LandingPageServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _barbershopServiceRepositoryMock = new Mock<IBarbershopServiceRepository>();
        _landingPageConfigRepositoryMock = new Mock<ILandingPageConfigRepository>();
        _landingPageServiceRepositoryMock = new Mock<ILandingPageServiceRepository>();
        _loggerMock = new Mock<ILogger<Application.UseCases.LandingPageService>>();

        _unitOfWorkMock.Setup(x => x.LandingPageConfigs).Returns(_landingPageConfigRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.LandingPageServices).Returns(_landingPageServiceRepositoryMock.Object);

        _service = new Application.UseCases.LandingPageService(
            _unitOfWorkMock.Object,
            _barbershopRepositoryMock.Object,
            _barbershopServiceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidBarbershopId_ShouldCreateLandingPage()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create("ABC23456"),
            "system");

        var service1 = BarbershopService.Create(barbershopId, "Corte", "Corte de cabelo", 30, 35.00m);
        var service2 = BarbershopService.Create(barbershopId, "Barba", "Barba completa", 20, 25.00m);

        _landingPageConfigRepositoryMock
            .Setup(x => x.ExistsForBarbershopAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _barbershopServiceRepositoryMock
            .Setup(x => x.ListAsync(barbershopId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BarbershopService> { service1, service2 });

        LandingPageConfig? savedConfig = null;
        _landingPageConfigRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<LandingPageConfig>(), It.IsAny<CancellationToken>()))
            .Callback<LandingPageConfig, CancellationToken>((config, _) => savedConfig = config)
            .Returns(Task.CompletedTask);

        // After insert, mock the GetByBarbershopIdWithServicesAsync to return a config with empty services
        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdWithServicesAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                // Return a new config that looks like what would be returned from DB
                var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");
                return config;
            });

        // Act
        var result = await _service.CreateAsync(barbershopId);

        // Assert
        result.Should().NotBeNull();
        result.BarbershopId.Should().Be(barbershopId);
        result.TemplateId.Should().Be(1);
        result.WhatsappNumber.Should().Be("11987654321");
        result.IsPublished.Should().BeTrue();

        _landingPageConfigRepositoryMock.Verify(
            x => x.InsertAsync(It.IsAny<LandingPageConfig>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _landingPageServiceRepositoryMock.Verify(
            x => x.InsertAsync(It.IsAny<Domain.Entities.LandingPageService>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_LandingPageAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _landingPageConfigRepositoryMock
            .Setup(x => x.ExistsForBarbershopAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(barbershopId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Landing page already exists for this barbershop");

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_BarbershopNotFound_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _landingPageConfigRepositoryMock
            .Setup(x => x.ExistsForBarbershopAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _barbershopRepositoryMock
            .Setup(x => x.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(barbershopId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Barbershop not found");

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByBarbershopIdAsync_ExistingLandingPage_ShouldReturnConfig()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdWithServicesAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _service.GetByBarbershopIdAsync(barbershopId);

        // Assert
        result.Should().NotBeNull();
        result.BarbershopId.Should().Be(barbershopId);
        result.TemplateId.Should().Be(1);
    }

    [Fact]
    public async Task GetByBarbershopIdAsync_LandingPageNotFound_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdWithServicesAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LandingPageConfig?)null);

        // Act
        Func<Task> act = async () => await _service.GetByBarbershopIdAsync(barbershopId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Landing page not found");
    }

    [Fact]
    public async Task GetPublicByCodeAsync_ValidCode_ShouldCallRepository()
    {
        // Arrange
        var code = "ABC23456";
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            Document.Create("12345678000190"),
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            UniqueCode.Create(code),
            "system");

        var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");

        // Use reflection to set the Barbershop property
        var barbershopProperty = typeof(LandingPageConfig).GetProperty("Barbershop");
        if (barbershopProperty != null)
        {
            barbershopProperty.SetValue(config, barbershop);
        }

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetPublicByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        var result = await _service.GetPublicByCodeAsync(code);

        // Assert
        result.Should().NotBeNull();
        result.Barbershop.Should().NotBeNull();
        result.Barbershop.Code.Should().Be(code);
        result.LandingPage.Should().NotBeNull();
        result.LandingPage.TemplateId.Should().Be(1);
    }

    [Fact]
    public async Task GetPublicByCodeAsync_EmptyCode_ShouldThrowException()
    {
        // Arrange
        var code = string.Empty;

        // Act
        Func<Task> act = async () => await _service.GetPublicByCodeAsync(code);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Code is required*");
    }

    [Fact]
    public async Task GetPublicByCodeAsync_LandingPageNotFound_ShouldThrowException()
    {
        // Arrange
        var code = "INVALID";

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetPublicByCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LandingPageConfig?)null);

        // Act
        Func<Task> act = async () => await _service.GetPublicByCodeAsync(code);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Landing page not found");
    }

    [Fact]
    public async Task UpdateConfigAsync_ValidInput_ShouldUpdateConfig()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");

        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Nova descrição",
            OpeningHours: "Segunda a Sexta: 10:00 - 20:00",
            InstagramUrl: "@barbearia",
            FacebookUrl: "https://facebook.com/barbearia",
            WhatsappNumber: "11999999999",
            Services: null);

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        await _service.UpdateConfigAsync(barbershopId, input);

        // Assert
        _landingPageConfigRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<LandingPageConfig>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigAsync_LandingPageNotFound_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var input = new UpdateLandingPageInput(2, null, null, null, null, null, null, null);

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LandingPageConfig?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateConfigAsync(barbershopId, input);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Landing page not found");

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServicesAsync_ValidInput_ShouldUpdateServices()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");

        var services = new List<ServiceDisplayInput>
        {
            new ServiceDisplayInput(Guid.NewGuid(), 1, true),
            new ServiceDisplayInput(Guid.NewGuid(), 2, true)
        };

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        await _service.UpdateServicesAsync(barbershopId, services);

        // Assert
        _landingPageServiceRepositoryMock.Verify(
            x => x.DeleteByLandingPageIdAsync(config.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _landingPageServiceRepositoryMock.Verify(
            x => x.InsertAsync(It.IsAny<Domain.Entities.LandingPageService>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateServicesAsync_NoVisibleServices_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var config = LandingPageConfig.Create(barbershopId, 1, "11987654321");

        var services = new List<ServiceDisplayInput>
        {
            new ServiceDisplayInput(Guid.NewGuid(), 1, false),
            new ServiceDisplayInput(Guid.NewGuid(), 2, false)
        };

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        // Act
        Func<Task> act = async () => await _service.UpdateServicesAsync(barbershopId, services);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("At least one service must be visible");

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServicesAsync_LandingPageNotFound_ShouldThrowException()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var services = new List<ServiceDisplayInput>
        {
            new ServiceDisplayInput(Guid.NewGuid(), 1, true)
        };

        _landingPageConfigRepositoryMock
            .Setup(x => x.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LandingPageConfig?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateServicesAsync(barbershopId, services);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Landing page not found");

        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExistsForBarbershopAsync_ExistingLandingPage_ShouldReturnTrue()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _landingPageConfigRepositoryMock
            .Setup(x => x.ExistsForBarbershopAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsForBarbershopAsync(barbershopId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsForBarbershopAsync_NonExistingLandingPage_ShouldReturnFalse()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _landingPageConfigRepositoryMock
            .Setup(x => x.ExistsForBarbershopAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ExistsForBarbershopAsync(barbershopId);

        // Assert
        result.Should().BeFalse();
    }
}
