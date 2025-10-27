// BarbApp.Application.Tests/UseCases/AuthenticateClienteUseCaseTests.cs
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

public class AuthenticateClienteUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepoMock;
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly AuthenticateClienteUseCase _useCase;

    public AuthenticateClienteUseCaseTests()
    {
        _barbershopRepoMock = new Mock<IBarbershopRepository>();
        _repositoryMock = new Mock<ICustomerRepository>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new AuthenticateClienteUseCase(
            _barbershopRepoMock.Object,
            _repositoryMock.Object,
            _tokenGeneratorMock.Object
        );
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task Execute_ExistingCustomerValidName_ShouldReturnAuthResponse()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "ABC23456",
            Telefone = "11987654321",
            Nome = "João Silva"
        };

        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var customer = Customer.Create(barbearia.Id, "11987654321", "João Silva");

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.Is<string>(id => id == customer.Id.ToString()),
                It.Is<string>(type => type == "Cliente"),
                It.Is<string>(email => email == customer.Telefone),
                It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
                It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("Cliente");
        result.BarbeariaId.Should().Be(barbearia.Id);
        result.NomeBarbearia.Should().Be(barbearia.Name);
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.Is<string>(id => id == customer.Id.ToString()),
            It.Is<string>(type => type == "Cliente"),
            It.Is<string>(email => email == customer.Telefone),
            It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
            It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)), Times.Once);
    }

    [Fact]
    public async Task Execute_NewCustomer_ShouldCreateAndReturnAuthResponse()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "ABC23456",
            Telefone = "11987654321",
            Nome = "João Silva"
        };

        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer customer, CancellationToken _) => customer);

        var expectedToken = new JwtToken("jwt-token", DateTime.UtcNow.AddHours(24));

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(
                It.IsAny<string>(),
                It.Is<string>(type => type == "Cliente"),
                It.Is<string>(email => email == input.Telefone),
                It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
                It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)))
            .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken.Value);
        result.TipoUsuario.Should().Be("Cliente");
        result.BarbeariaId.Should().Be(barbearia.Id);
        result.NomeBarbearia.Should().Be(barbearia.Name);
        result.ExpiresAt.Should().Be(expectedToken.ExpiresAt);

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(
            It.IsAny<string>(),
            It.Is<string>(type => type == "Cliente"),
            It.Is<string>(email => email == input.Telefone),
            It.Is<Guid?>(barbeariaId => barbeariaId == barbearia.Id),
            It.Is<string>(barbeariaCode => barbeariaCode == barbearia.Code.Value)), Times.Once);
    }

    [Fact]
    public async Task Execute_InvalidUniqueCode_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "INVALID",
            Telefone = "11987654321",
            Nome = "João Silva"
        };

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Código da barbearia inválido");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ExistingCustomerWrongName_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "ABC23456",
            Telefone = "11987654321",
            Nome = "João Silva"
        };

        var barbeariaCode = UniqueCode.Create("ABC23456");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var customer = Customer.Create(barbearia.Id, "11987654321", "Maria Silva"); // Different name

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _repositoryMock
            .Setup(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BarbApp.Domain.Exceptions.UnauthorizedAccessException>()
            .WithMessage("Nome não corresponde ao telefone cadastrado");

        _barbershopRepoMock.Verify(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetByTelefoneAndBarbeariaIdAsync(input.Telefone, barbearia.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }
}