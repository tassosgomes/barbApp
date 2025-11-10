using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class ClientAuthenticationIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _testBarbeariaId;
    private string _testBarbeariaCode;

    public ClientAuthenticationIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new IntegrationTestWebAppFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _factory.EnsureDatabaseInitialized();
        await SetupTestData();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SetupTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        // Create test barbearia
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document = Document.Create("11111111000191");
        var code = UniqueCode.Create("TESTAUTH");
        var barbearia = Barbershop.Create(
            "Barbearia Test Auth",
            document,
            "(11) 98765-4321",
            "João Silva",
            "joao@teste.com",
            address,
            code,
            "test"
        );

        dbContext.Addresses.Add(address);
        dbContext.Barbershops.Add(barbearia);
        await dbContext.SaveChangesAsync();

        _testBarbeariaId = barbearia.Id;
        _testBarbeariaCode = code.Value;
    }

    [Fact]
    public async Task LoginCliente_NovoCliente_DeveCriarClienteERetornarToken()
    {
        // Arrange
        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = _testBarbeariaCode,
            Telefone = "11999999999",
            Nome = "Cliente Novo"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("Cliente");
        authResponse.BarbeariaId.Should().Be(_testBarbeariaId);
        authResponse.NomeBarbearia.Should().Be("Barbearia Test Auth");

        // Verify client was created in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var cliente = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Telefone == "11999999999" && c.BarbeariaId == _testBarbeariaId);

        cliente.Should().NotBeNull();
        cliente!.Name.Should().Be("Cliente Novo");
    }

    [Fact]
    public async Task LoginCliente_ClienteExistente_NomeCorreto_DeveRetornarToken()
    {
        // Arrange - Create customer first
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var customer = Customer.Create(_testBarbeariaId, "11888888888", "Cliente Existente");
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = _testBarbeariaCode,
            Telefone = "11888888888",
            Nome = "Cliente Existente"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("Cliente");
    }

    [Fact]
    public async Task LoginCliente_ClienteExistente_NomeIncorreto_DeveRetornar401()
    {
        // Arrange - Create customer first
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var customer = Customer.Create(_testBarbeariaId, "11999999999", "Cliente Existente");
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = _testBarbeariaCode,
            Telefone = "11999999999",
            Nome = "Nome Errado"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginCliente_CodigoBarbeariaInvalido_DeveRetornar400()
    {
        // Arrange
        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = "INVALID",
            Telefone = "11666666666",
            Nome = "Cliente Teste"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginCliente_MesmoTelefone_DiferentesBarbearias_DeveCriarClientesSeparados()
    {
        // Arrange - Create second barbearia
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var address2 = Address.Create("01310101", "Av. Brigadeiro", "1001", null, "Jardim Paulista", "São Paulo", "SP");
        var document2 = Document.Create("22222222000192");
        var code2 = UniqueCode.Create("TESTAUT2");
        var barbearia2 = Barbershop.Create(
            "Barbearia Test Auth 2",
            document2,
            "(11) 98765-4322",
            "Maria Silva",
            "maria@teste.com",
            address2,
            code2,
            "test"
        );

        dbContext.Addresses.Add(address2);
        dbContext.Barbershops.Add(barbearia2);
        await dbContext.SaveChangesAsync();

        // Act - Login same phone in both barbearias
        var loginInput1 = new LoginClienteInput
        {
            CodigoBarbearia = _testBarbeariaCode,
            Telefone = "11555555555",
            Nome = "Cliente Multi"
        };

        var loginInput2 = new LoginClienteInput
        {
            CodigoBarbearia = code2.Value,
            Telefone = "11555555555",
            Nome = "Cliente Multi"
        };

        var response1 = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput1);
        var response2 = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify two separate clients were created
        var clientes = await dbContext.Customers
            .Where(c => c.Telefone == "11555555555")
            .ToListAsync();

        clientes.Should().HaveCount(2);
        clientes[0].BarbeariaId.Should().NotBe(clientes[1].BarbeariaId);
    }

    [Theory]
    [InlineData("", "11999999999", "Cliente", "Código da barbearia é obrigatório")]
    [InlineData("TESTAUTHX", "", "Cliente", "Telefone é obrigatório")]
    [InlineData("TESTAUTHX", "11999999999", "", "Nome é obrigatório")]
    [InlineData("TESTAUTHX", "123", "Cliente", "Telefone deve conter 10 ou 11 dígitos")]
    [InlineData("TESTAUTHX", "119999999999", "Cliente", "Telefone deve conter 10 ou 11 dígitos")]
    public async Task LoginCliente_DadosInvalidos_DeveRetornar400(
        string codigo, string telefone, string nome, string expectedError)
    {
        // Arrange
        var loginInput = new LoginClienteInput
        {
            CodigoBarbearia = codigo,
            Telefone = telefone,
            Nome = nome
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(expectedError);
    }
}