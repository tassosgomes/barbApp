// BarbApp.Application.Tests/UseCases/TrocarContextoBarbeiroUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class TrocarContextoBarbeiroUseCaseTests
{
    private readonly Mock<IBarberRepository> _repositoryMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly TrocarContextoBarbeiroUseCase _useCase;

    public TrocarContextoBarbeiroUseCaseTests()
    {
        _repositoryMock = new Mock<IBarberRepository>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _tenantContextMock = new Mock<ITenantContext>();
        _useCase = new TrocarContextoBarbeiroUseCase(
            _repositoryMock.Object,
            _tokenGeneratorMock.Object,
            _tenantContextMock.Object
        );
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task Execute_ValidBarberInNewBarbearia_ShouldReturnAuthResponse()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        };

        var userId = "11987654321"; // telefone is used as userId
        var currentRole = "Barbeiro";
        var telefone = userId;

        var barbeariaCode = UniqueCode.Create("XYZ98765");
        var newBarbearia = CreateTestBarbershop("Nova Barbearia", barbeariaCode);
        var barber = Barber.Create(input.NovaBarbeariaId, "João Silva", "joao@test.com", "hashedpassword", telefone);

        // Set navigation property for the test
        barber.GetType().GetProperty("Barbearia")?.SetValue(barber, newBarbearia);

        _tenantContextMock
            .Setup(x => x.UserId)
            .Returns(userId);

        _tenantContextMock
            .Setup(x => x.Role)
            .Returns(currentRole);

        _repositoryMock
            .Setup(x => x.GetByTelefoneAndBarbeariaIdAsync(telefone, input.NovaBarbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.Is<string>(id => id == barber.Id.ToString()),
                It.Is<string>(type => type == "Barbeiro"),
                It.Is<string>(email => email == barber.Phone),
                It.Is<Guid?>(barbeariaId => barbeariaId == barber.BarbeariaId),
                It.Is<string>(barbeariaCode => barbeariaCode == newBarbearia.Code.Value)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("Barbeiro");
        result.BarbeariaId.Should().Be(barber.BarbeariaId);
        result.NomeBarbearia.Should().Be(barber.Barbearia.Name);
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _tenantContextMock.Verify(x => x.UserId, Times.Once);
        _tenantContextMock.Verify(x => x.Role, Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(telefone, input.NovaBarbeariaId, It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.Is<string>(id => id == barber.Id.ToString()),
            It.Is<string>(type => type == "Barbeiro"),
            It.Is<string>(email => email == barber.Phone),
            It.Is<Guid?>(barbeariaId => barbeariaId == barber.BarbeariaId),
            It.Is<string>(barbeariaCode => barbeariaCode == newBarbearia.Code.Value)), Times.Once);
    }

    [Fact]
    public async Task Execute_UserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        };

        _tenantContextMock
            .Setup(x => x.UserId)
            .Returns((string?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Usuário não autenticado como barbeiro");

        _tenantContextMock.Verify(x => x.UserId, Times.Once);
        _tenantContextMock.Verify(x => x.Role, Times.Never);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UserNotBarbeiro_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        };

        _tenantContextMock
            .Setup(x => x.UserId)
            .Returns("some-user-id");

        _tenantContextMock
            .Setup(x => x.Role)
            .Returns("Cliente");

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Usuário não autenticado como barbeiro");

        _tenantContextMock.Verify(x => x.UserId, Times.Once);
        _tenantContextMock.Verify(x => x.Role, Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_BarberNotFoundInNewBarbearia_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        };

        var userId = "11987654321";
        var currentRole = "Barbeiro";

        _tenantContextMock
            .Setup(x => x.UserId)
            .Returns(userId);

        _tenantContextMock
            .Setup(x => x.Role)
            .Returns(currentRole);

        _repositoryMock
            .Setup(x => x.GetByTelefoneAndBarbeariaIdAsync(userId, input.NovaBarbeariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.NotFoundException>()
            .WithMessage("Barbeiro não encontrado na barbearia especificada");

        _tenantContextMock.Verify(x => x.UserId, Times.Once);
        _tenantContextMock.Verify(x => x.Role, Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(userId, input.NovaBarbeariaId, It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }
}