using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BarbApp.IntegrationTests.Auth;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_AdminCentral_ShouldReturn200AndAuthResponse()
    {
        var mockUseCase = new Mock<IAuthenticateAdminCentralUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<LoginAdminCentralInput>(), default))
            .ReturnsAsync(new AuthResponse
            {
                Token = "token-admin-central",
                TipoUsuario = "AdminCentral",
                BarbeariaId = null,
                NomeBarbearia = string.Empty,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/admin-central/login", new LoginAdminCentralInput
        {
            Email = "admin@central.com",
            Senha = "123456"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.TipoUsuario.Should().Be("AdminCentral");
        mockUseCase.VerifyAll();
    }

    [Fact]
    public async Task Login_AdminBarbearia_ShouldReturn200AndAuthResponse()
    {
        var mockUseCase = new Mock<IAuthenticateAdminBarbeariaUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<LoginAdminBarbeariaInput>(), default))
            .ReturnsAsync(new AuthResponse
            {
                Token = "token-admin-barbearia",
                TipoUsuario = "AdminBarbearia",
                BarbeariaId = Guid.NewGuid(),
                NomeBarbearia = "Barbearia XPTO",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/admin-barbearia/login", new LoginAdminBarbeariaInput
        {
            Email = "admin@barb.com",
            Senha = "123456",
            Codigo = "BARB123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.TipoUsuario.Should().Be("AdminBarbearia");
        mockUseCase.VerifyAll();
    }

    [Fact]
    public async Task Login_Barbeiro_ShouldReturn200AndAuthResponse()
    {
        var mockUseCase = new Mock<IAuthenticateBarbeiroUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<LoginBarbeiroInput>(), default))
            .ReturnsAsync(new AuthResponse
            {
                Token = "token-barbeiro",
                TipoUsuario = "Barbeiro",
                BarbeariaId = Guid.NewGuid(),
                NomeBarbearia = "Barb",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/barbeiro/login", new LoginBarbeiroInput
        {
            Codigo = "BARB123",
            Telefone = "11999998888"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.TipoUsuario.Should().Be("Barbeiro");
        mockUseCase.VerifyAll();
    }

    [Fact]
    public async Task Login_Cliente_ShouldReturn200AndAuthResponse()
    {
        var mockUseCase = new Mock<IAuthenticateClienteUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<LoginClienteInput>(), default))
            .ReturnsAsync(new AuthResponse
            {
                Token = "token-cliente",
                TipoUsuario = "Cliente",
                BarbeariaId = Guid.NewGuid(),
                NomeBarbearia = "Barb",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/cliente/login", new LoginClienteInput
        {
            Codigo = "BARB123",
            Telefone = "11999998888",
            Nome = "Cliente Teste"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.TipoUsuario.Should().Be("Cliente");
        mockUseCase.VerifyAll();
    }

    [Fact]
    public async Task List_Barbeiros_Authorized_ShouldReturn200WithList()
    {
        var mockUseCase = new Mock<IListBarbeirosBarbeariaUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(default))
            .ReturnsAsync(new List<BarberInfo>
            {
                new() { Id = Guid.NewGuid(), Nome = "João", Telefone = "11911111111", BarbeariaId = Guid.NewGuid(), NomeBarbearia = "Barb A" },
                new() { Id = Guid.NewGuid(), Nome = "Pedro", Telefone = "11922222222", BarbeariaId = Guid.NewGuid(), NomeBarbearia = "Barb B" }
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.GetAsync("/api/auth/barbeiros");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<BarberInfo>>();
        list.Should().NotBeNull();
        list!.Count.Should().Be(2);
        mockUseCase.VerifyAll();
    }

    [Fact]
    public async Task Trocar_Contexto_Authorized_ShouldReturn200AndAuthResponse()
    {
        var mockUseCase = new Mock<ITrocarContextoBarbeiroUseCase>(MockBehavior.Strict);
        mockUseCase
            .Setup(x => x.ExecuteAsync(It.IsAny<TrocarContextoInput>(), default))
            .ReturnsAsync(new AuthResponse
            {
                Token = "token-barbeiro-novo",
                TipoUsuario = "Barbeiro",
                BarbeariaId = Guid.NewGuid(),
                NomeBarbearia = "Nova Barbearia",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddSingleton(mockUseCase.Object);
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/barbeiro/trocar-contexto", new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.TipoUsuario.Should().Be("Barbeiro");
        mockUseCase.VerifyAll();
    }
}

// Test authentication handler to bypass JWT in authorized endpoints
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("user_type", "Barbeiro"),
            new Claim("barbearia_id", Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
