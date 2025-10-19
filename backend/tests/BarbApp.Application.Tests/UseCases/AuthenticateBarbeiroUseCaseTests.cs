// BarbApp.Application.Tests/UseCases/AuthenticateBarbeiroUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class AuthenticateBarbeiroUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepoMock;
    private readonly Mock<IBarberRepository> _repositoryMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthenticateBarbeiroUseCase _useCase;

    public AuthenticateBarbeiroUseCaseTests()
    {
        _barbershopRepoMock = new Mock<IBarbershopRepository>();
        _repositoryMock = new Mock<IBarberRepository>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _useCase = new AuthenticateBarbeiroUseCase(
            _barbershopRepoMock.Object,
            _repositoryMock.Object,
            _tokenGeneratorMock.Object,
            _passwordHasherMock.Object
        );
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task Execute_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = "joao@test.com",
            Password = "password123"
        };

        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var barber = Barber.Create(barbearia.Id, "João Silva", "joao@test.com", "hashedpassword", "11987654321");

        _repositoryMock
            .Setup(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Password, barber.PasswordHash))
            .Returns(true);

        _barbershopRepoMock
            .Setup(x => x.GetByIdAsync(barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.Is<string>(id => id == barber.Id.ToString()),
                It.Is<string>(type => type == "Barbeiro"),
                It.Is<string>(email => email == barber.Email),
                It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
                It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("Barbeiro");
        result.BarbeariaId.Should().Be(barbearia.Id);
        result.NomeBarbearia.Should().Be(barbearia.Name);
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _repositoryMock.Verify(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Password, barber.PasswordHash), Times.Once);
        _barbershopRepoMock.Verify(x => x.GetByIdAsync(barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.Is<string>(id => id == barber.Id.ToString()),
            It.Is<string>(type => type == "Barbeiro"),
            It.Is<string>(email => email == barber.Email),
            It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
            It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)), Times.Once);
    }

    [Fact]
    public async Task Execute_InvalidEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = "invalid@test.com",
            Password = "password123"
        };

        _repositoryMock
            .Setup(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("E-mail ou senha inválidos");

        _repositoryMock.Verify(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _barbershopRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = "joao@test.com",
            Password = "wrongpassword"
        };

        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var barber = Barber.Create(barbearia.Id, "João Silva", "joao@test.com", "hashedpassword", "11987654321");

        _repositoryMock
            .Setup(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Password, barber.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("E-mail ou senha inválidos");

        _repositoryMock.Verify(x => x.GetByEmailGlobalAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Password, barber.PasswordHash), Times.Once);
        _barbershopRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }
}