// BarbApp.Application.Tests/UseCases/AuthenticateAdminBarbeariaUseCaseTests.cs
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

public class AuthenticateAdminBarbeariaUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepoMock;
    private readonly Mock<IAdminBarbeariaUserRepository> _repositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly AuthenticateAdminBarbeariaUseCase _useCase;

    public AuthenticateAdminBarbeariaUseCaseTests()
    {
        _barbershopRepoMock = new Mock<IBarbershopRepository>();
        _repositoryMock = new Mock<IAdminBarbeariaUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new AuthenticateAdminBarbeariaUseCase(
            _barbershopRepoMock.Object,
            _repositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Codigo = "ABC23456",
            Email = "admin@barbearia.com",
            Senha = "password123"
        };

        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var code = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            code,
            "admin-user-id"
        );
        var user = AdminBarbeariaUser.Create(barbearia.Id, "admin@barbearia.com", "hashedpassword", "Admin Barbearia");

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Senha, user.PasswordHash))
            .Returns(true);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.Is<string>(id => id == user.Id.ToString()),
                It.Is<string>(type => type == "AdminBarbearia"),
                It.Is<string>(email => email == user.Email),
                It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
                It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("AdminBarbearia");
        result.BarbeariaId.Should().Be(barbearia.Id);
        result.NomeBarbearia.Should().Be(barbearia.Name);
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Senha, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.Is<string>(id => id == user.Id.ToString()),
            It.Is<string>(type => type == "AdminBarbearia"),
            It.Is<string>(email => email == user.Email),
            It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
            It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)), Times.Once);
    }

    [Fact]
    public async Task Execute_InvalidUniqueCode_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Codigo = "INVALID",
            Email = "admin@barbearia.com",
            Senha = "password123"
        };

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Código da barbearia inválido");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByEmailAndBarbeariaIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_InactiveBarbearia_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Codigo = "ABC23456",
            Email = "admin@barbearia.com",
            Senha = "password123"
        };

        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var code = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            code,
            "admin-user-id"
        );
        barbearia.Deactivate(); // Make barbearia inactive

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Código da barbearia inválido");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByEmailAndBarbeariaIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Codigo = "ABC23456",
            Email = "nonexistent@barbearia.com",
            Senha = "password123"
        };

        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var code = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            code,
            "admin-user-id"
        );

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Codigo = "ABC23456",
            Email = "admin@barbearia.com",
            Senha = "wrongpassword"
        };

        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var code = UniqueCode.Create("ABC23456");
        var barbearia = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            code,
            "admin-user-id"
        );
        var user = AdminBarbeariaUser.Create(barbearia.Id, "admin@barbearia.com", "hashedpassword", "Admin Barbearia");

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Senha, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.Codigo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByEmailAndBarbeariaIdAsync(input.Email, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Senha, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }
}