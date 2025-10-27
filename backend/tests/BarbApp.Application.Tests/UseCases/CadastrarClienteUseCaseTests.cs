// BarbApp.Application.Tests/UseCases/CadastrarClienteUseCaseTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace BarbApp.Application.Tests.UseCases;

public class CadastrarClienteUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly CadastrarClienteUseCase _useCase;

    public CadastrarClienteUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new CadastrarClienteUseCase(
            _barbershopRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object
        );
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarClienteERetornarToken()
    {
        // Arrange
        var input = new CadastrarClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Nome = "João Silva",
            Telefone = "11987654321"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _clienteRepositoryMock
            .Setup(x => x.ExisteAsync(barbearia.Id, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var token = new JwtToken("fake-jwt-token", DateTime.UtcNow.AddHours(24));
        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), "Cliente", It.IsAny<string>(), barbearia.Id, barbearia.Code.Value))
            .Returns(token);

        // Act
        var result = await _useCase.Handle(input);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Cliente.Nome.Should().Be("João Silva");
        result.Cliente.Telefone.Should().Be("11987654321");
        result.Barbearia.Nome.Should().Be("Barbearia Teste");
        result.Barbearia.Codigo.Should().Be("TEST2345");

        _clienteRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComCodigoBarbeariaInvalido_DeveLancarBarbeariaNotFoundException()
    {
        // Arrange
        var input = new CadastrarClienteInput
        {
            CodigoBarbearia = "INVALID8",
            Nome = "João Silva",
            Telefone = "11987654321"
        };

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbeariaNotFoundException>(() => _useCase.Handle(input));
    }

    [Fact]
    public async Task Handle_ComBarbeariaInativa_DeveLancarBarbeariaNotFoundException()
    {
        // Arrange
        var input = new CadastrarClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Nome = "João Silva",
            Telefone = "11987654321"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        barbearia.Deactivate(); // Torna inativa

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        // Act & Assert
        await Assert.ThrowsAsync<BarbeariaNotFoundException>(() => _useCase.Handle(input));
    }

    [Fact]
    public async Task Handle_ComTelefoneDuplicado_DeveLancarClienteJaExisteException()
    {
        // Arrange
        var input = new CadastrarClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Nome = "João Silva",
            Telefone = "11987654321"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _clienteRepositoryMock
            .Setup(x => x.ExisteAsync(barbearia.Id, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ClienteJaExisteException>(() => _useCase.Handle(input));
    }
}