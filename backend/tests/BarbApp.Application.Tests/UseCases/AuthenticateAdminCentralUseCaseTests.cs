// BarbApp.Application.Tests/UseCases/AuthenticateAdminCentralUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class AuthenticateAdminCentralUseCaseTests
{
    private readonly Mock<IAdminCentralUserRepository> _repositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly AuthenticateAdminCentralUseCase _useCase;

    public AuthenticateAdminCentralUseCaseTests()
    {
        _repositoryMock = new Mock<IAdminCentralUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new AuthenticateAdminCentralUseCase(
            _repositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = "admin@barbapp.com",
            Senha = "password123"
        };

        var user = AdminCentralUser.Create("admin@barbapp.com", "hashedpassword", "Admin Central");

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Senha, user.PasswordHash))
            .Returns(true);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.Is<string>(id => id == user.Id.ToString()),
                It.Is<string>(type => type == "AdminCentral"),
                It.Is<string>(email => email == user.Email),
                It.Is<Guid?>(barbeariaId => barbeariaId == null)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("AdminCentral");
        result.BarbeariaId.Should().BeNull();
        result.NomeBarbearia.Should().BeEmpty();
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _repositoryMock.Verify(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Senha, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.Is<string>(id => id == user.Id.ToString()),
            It.Is<string>(type => type == "AdminCentral"),
            It.Is<string>(email => email == user.Email),
            It.Is<Guid?>(barbeariaId => barbeariaId == null)), Times.Once);
    }

    [Fact]
    public async Task Execute_UserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = "nonexistent@barbapp.com",
            Senha = "password123"
        };

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminCentralUser?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");

        _repositoryMock.Verify(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Execute_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = "admin@barbapp.com",
            Senha = "wrongpassword"
        };

        var user = AdminCentralUser.Create("admin@barbapp.com", "hashedpassword", "Admin Central");

        _repositoryMock
            .Setup(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(input.Senha, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");

        _repositoryMock.Verify(x => x.GetByEmailAsync(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(input.Senha, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
    }
}