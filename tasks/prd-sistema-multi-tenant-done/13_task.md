---
status: completed
parallelizable: true
blocked_by: ["11.0"]
---

<task_context>
<domain>Qualidade e Testes</domain>
<type>Testes</type>
<scope>Testes de Integração</scope>
<complexity>alta</complexity>
<dependencies>TestContainers, WebApplicationFactory, xUnit</dependencies>
<unblocks>"14.0"</unblocks>
</task_context>

# Tarefa 13.0: Implementar Testes de Integração ✅ CONCLUÍDA

## Visão Geral
Criar suite completa de testes de integração usando TestContainers para database isolation, WebApplicationFactory para API testing, cobrindo todos os fluxos de autenticação e isolamento multi-tenant.

<requirements>
- TestContainers para PostgreSQL containerizado
- WebApplicationFactory para testes de API
- Testes de todos os endpoints de autenticação
- Testes de isolamento multi-tenant
- Testes de autorização e permissões
- Testes de validação de DTOs
- Testes de geração e validação JWT
- Setup e teardown apropriado de fixtures
- Cobertura de código >80%
</requirements>

## Subtarefas
- [x] 13.1 Configurar TestContainers e WebApplicationFactory
- [x] 13.2 Criar fixtures de teste e helpers
- [x] 13.3 Implementar testes de autenticação AdminCentral
- [x] 13.4 Implementar testes de autenticação AdminBarbearia
- [x] 13.5 Implementar testes de autenticação Barbeiro
- [x] 13.6 Implementar testes de autenticação Cliente
- [x] 13.7 Implementar testes de listagem de barbeiros
- [x] 13.8 Implementar testes de troca de contexto
- [x] 13.9 Implementar testes de isolamento multi-tenant
- [x] 13.10 Implementar testes de validação e erros
- [x] 13.11 Gerar relatório de cobertura

## Sequenciamento
- **Bloqueado por**: 11.0 (Configuração de API)
- **Desbloqueia**: 14.0 (Validação End-to-End)
- **Paralelizável**: Sim (pode ser desenvolvido em paralelo com 12.0)

## Detalhes de Implementação

### Test Infrastructure Setup

```csharp
// IntegrationTestWebAppFactory.cs
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("barbapp_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test DbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Build service provider and migrate database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
```

### Test Helpers

```csharp
// TestHelper.cs
public static class TestHelper
{
    public static async Task<(Guid id, string email, string senha)> CreateAdminCentralAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher)
    {
        var email = $"admin-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";

        var admin = new AdminCentralUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            Nome = "Test Admin",
            SenhaHash = passwordHasher.HashPassword(senha)
        };

        await context.AdminCentralUsers.AddAsync(admin);
        await context.SaveChangesAsync();

        return (admin.Id, email, senha);
    }

    public static async Task<(Guid barbeariaId, Guid adminId, string email, string senha)>
        CreateAdminBarbeariaAsync(
            AppDbContext context,
            IPasswordHasher passwordHasher)
    {
        var barbearia = new Barbearia
        {
            Id = Guid.NewGuid(),
            Nome = $"Test Barbearia {Guid.NewGuid()}"
        };
        await context.Barbearias.AddAsync(barbearia);

        var email = $"admin-barb-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";

        var admin = new AdminBarbeariaUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            Nome = "Test Admin Barbearia",
            SenhaHash = passwordHasher.HashPassword(senha),
            BarbeariaId = barbearia.Id
        };

        await context.AdminBarbeariaUsers.AddAsync(admin);
        await context.SaveChangesAsync();

        return (barbearia.Id, admin.Id, email, senha);
    }

    public static async Task<string> GetAuthTokenAsync(
        HttpClient client,
        string endpoint,
        object loginData)
    {
        var response = await client.PostAsJsonAsync(endpoint, loginData);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }
}
```

### Authentication Tests

```csharp
// AuthenticationTests.cs
public class AuthenticationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthenticationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LoginAdminCentral_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (_, email, senha) = await TestHelper.CreateAdminCentralAsync(context, passwordHasher);

        var loginInput = new LoginAdminCentralInput
        {
            Email = email,
            Senha = senha
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("AdminCentral");
        authResponse.BarbeariaId.Should().BeNull();
    }

    [Fact]
    public async Task LoginAdminCentral_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (_, email, _) = await TestHelper.CreateAdminCentralAsync(context, passwordHasher);

        var loginInput = new LoginAdminCentralInput
        {
            Email = email,
            Senha = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginAdminBarbearia_WithValidCredentials_ReturnsTokenWithBarbeariaId()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var (barbeariaId, _, email, senha) =
            await TestHelper.CreateAdminBarbeariaAsync(context, passwordHasher);

        var loginInput = new LoginAdminBarbeariaInput
        {
            Email = email,
            Senha = senha,
            BarbeariaId = barbeariaId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-barbearia/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.TipoUsuario.Should().Be("AdminBarbearia");
        authResponse.BarbeariaId.Should().Be(barbeariaId);
    }

    [Theory]
    [InlineData("", "Test@123", "Email é obrigatório")]
    [InlineData("invalid-email", "Test@123", "Email inválido")]
    [InlineData("test@test.com", "", "Senha é obrigatória")]
    [InlineData("test@test.com", "123", "Senha deve ter no mínimo 6 caracteres")]
    public async Task LoginAdminCentral_WithInvalidInput_ReturnsBadRequest(
        string email, string senha, string expectedError)
    {
        // Arrange
        var loginInput = new LoginAdminCentralInput
        {
            Email = email,
            Senha = senha
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin-central/login", loginInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain(expectedError);
    }
}
```

### Multi-Tenant Isolation Tests

```csharp
// MultiTenantIsolationTests.cs
public class MultiTenantIsolationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public MultiTenantIsolationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListBarbeiros_ReturnsOnlyBarbeirosFromUserBarbearia()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Create Barbearia 1 with admin and barbers
        var (barb1Id, _, emailAdmin1, senhaAdmin1) =
            await TestHelper.CreateAdminBarbeariaAsync(context, passwordHasher);

        var barber1_1 = new Barber
        {
            Id = Guid.NewGuid(),
            Email = "barber1_1@test.com",
            Nome = "Barber 1-1",
            SenhaHash = passwordHasher.HashPassword("Test@123"),
            BarbeariaId = barb1Id
        };
        await context.Barbers.AddAsync(barber1_1);

        // Create Barbearia 2 with admin and barbers
        var (barb2Id, _, _, _) =
            await TestHelper.CreateAdminBarbeariaAsync(context, passwordHasher);

        var barber2_1 = new Barber
        {
            Id = Guid.NewGuid(),
            Email = "barber2_1@test.com",
            Nome = "Barber 2-1",
            SenhaHash = passwordHasher.HashPassword("Test@123"),
            BarbeariaId = barb2Id
        };
        await context.Barbers.AddAsync(barber2_1);
        await context.SaveChangesAsync();

        // Login as Admin of Barbearia 1
        var token = await TestHelper.GetAuthTokenAsync(
            _client,
            "/api/auth/admin-barbearia/login",
            new LoginAdminBarbeariaInput
            {
                Email = emailAdmin1,
                Senha = senhaAdmin1,
                BarbeariaId = barb1Id
            });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var barbeiros = await response.Content.ReadFromJsonAsync<List<BarberInfo>>();
        barbeiros.Should().NotBeNull();
        barbeiros!.Count.Should().Be(1);
        barbeiros[0].BarbeariaId.Should().Be(barb1Id);
        barbeiros[0].Email.Should().Be("barber1_1@test.com");
    }

    [Fact]
    public async Task TrocarContexto_WithValidBarbearia_ReturnsNewToken()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Create 2 barbearias
        var barb1 = new Barbearia { Id = Guid.NewGuid(), Nome = "Barbearia 1" };
        var barb2 = new Barbearia { Id = Guid.NewGuid(), Nome = "Barbearia 2" };
        await context.Barbearias.AddRangeAsync(barb1, barb2);

        // Create barber in both barbearias
        var email = $"barber-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";
        var senhaHash = passwordHasher.HashPassword(senha);

        var barber1 = new Barber
        {
            Id = Guid.NewGuid(),
            Email = email,
            Nome = "Test Barber",
            SenhaHash = senhaHash,
            BarbeariaId = barb1.Id
        };

        var barber2 = new Barber
        {
            Id = Guid.NewGuid(),
            Email = email,
            Nome = "Test Barber",
            SenhaHash = senhaHash,
            BarbeariaId = barb2.Id
        };

        await context.Barbers.AddRangeAsync(barber1, barber2);
        await context.SaveChangesAsync();

        // Login to Barbearia 1
        var token = await TestHelper.GetAuthTokenAsync(
            _client,
            "/api/auth/barbeiro/login",
            new LoginBarbeiroInput
            {
                Email = email,
                Senha = senha,
                BarbeariaId = barb1.Id
            });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act - Switch to Barbearia 2
        var response = await _client.PostAsJsonAsync(
            "/api/auth/barbeiro/trocar-contexto",
            new TrocarContextoInput { NovaBarbeariaId = barb2.Id });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.BarbeariaId.Should().Be(barb2.Id);
        authResponse.NomeBarbearia.Should().Be("Barbearia 2");
        authResponse.Token.Should().NotBe(token);
    }
}
```

### Authorization Tests

```csharp
// AuthorizationTests.cs
public class AuthorizationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthorizationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListBarbeiros_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrocarContexto_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var input = new TrocarContextoInput { NovaBarbeariaId = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/barbeiro/trocar-contexto", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- ✅ Fixtures e helpers reutilizáveis

## Critérios de Sucesso
- ✅ TestContainers configurado e funcionando
- ✅ WebApplicationFactory isolando testes corretamente
- ✅ Todos os endpoints de autenticação testados
- ✅ Isolamento multi-tenant validado
- ✅ Autorização e permissões testadas
- ✅ Validação de DTOs testada
- ✅ Geração e validação JWT testadas
- ✅ Cobertura de código >80%
- ✅ Testes são rápidos e confiáveis
- ✅ Fixtures e helpers reutilizáveis

## Checklist de Conclusão da Tarefa
- [x] 13.0 Implementação completada ✅ CONCLUÍDA
  - [x] 13.1 Configurar TestContainers e WebApplicationFactory
  - [x] 13.2 Criar fixtures de teste e helpers
  - [x] 13.3 Implementar testes de autenticação AdminCentral
  - [x] 13.4 Implementar testes de autenticação AdminBarbearia
  - [x] 13.5 Implementar testes de autenticação Barbeiro
  - [x] 13.6 Implementar testes de autenticação Cliente
  - [x] 13.7 Implementar testes de listagem de barbeiros
  - [x] 13.8 Implementar testes de troca de contexto
  - [x] 13.9 Implementar testes de isolamento multi-tenant
  - [x] 13.10 Implementar testes de validação e erros
  - [x] 13.11 Gerar relatório de cobertura
- [x] 13.0 Definição da tarefa, PRD e tech spec validados
- [x] 13.0 Análise de regras e conformidade verificadas
- [x] 13.0 Revisão de código completada
- [x] 13.0 Pronto para deploy

## Tempo Estimado

## Referências
- TechSpec: Seção "4.8 Fase 1.8: Testes de Integração"
- PRD: Seção "Requisitos de Qualidade"
- TestContainers Documentation
- xUnit Best Practices
