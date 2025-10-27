// BarbApp.Application.Tests/UseCases/LoginClienteUseCaseTests.cs
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

public class LoginClienteUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginClienteUseCase _useCase;

    public LoginClienteUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new LoginClienteUseCase(
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

    private static Cliente CreateTestCliente(Guid barbeariaId, string nome, string telefone)
    {
        return Cliente.Create(barbeariaId, nome, telefone);
    }

    [Fact]
    public async Task Handle_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Telefone = "11987654321",
            Nome = "João Silva"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var cliente = CreateTestCliente(barbearia.Id, "João Silva", "11987654321");

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _clienteRepositoryMock
            .Setup(x => x.GetByTelefoneAsync(barbearia.Id, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

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
    }

    [Fact]
    public async Task Handle_ComCodigoBarbeariaInvalido_DeveLancarBarbeariaNotFoundException()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "INVALID8",
            Telefone = "11987654321",
            Nome = "João Silva"
        };

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbeariaNotFoundException>(() => _useCase.Handle(input));
    }

    [Fact]
    public async Task Handle_ComTelefoneNaoCadastrado_DeveLancarUnauthorizedException()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Telefone = "11987654321",
            Nome = "João Silva"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _clienteRepositoryMock
            .Setup(x => x.GetByTelefoneAsync(barbearia.Id, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _useCase.Handle(input));
        exception.Message.Should().Be("Telefone não cadastrado. Faça seu primeiro agendamento!");
    }

    [Fact]
    public async Task Handle_ComNomeIncorreto_DeveLancarUnauthorizedException()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            CodigoBarbearia = "TEST2345",
            Telefone = "11987654321",
            Nome = "João Silva"
        };
        var barbeariaCode = UniqueCode.Create("TEST2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        var cliente = CreateTestCliente(barbearia.Id, "Maria Silva", "11987654321"); // Nome diferente

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);

        _clienteRepositoryMock
            .Setup(x => x.GetByTelefoneAsync(barbearia.Id, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _useCase.Handle(input));
        exception.Message.Should().Be("Nome incorreto");
    }
}