---
status: pending
parallelizable: false
blocked_by: ["4.0", "8.0"]
---

<task_context>
<domain>backend/testing</domain>
<type>testing</type>
<scope>integration</scope>
<complexity>medium</complexity>
<dependencies>database, http_server</dependencies>
<unblocks>14.0</unblocks>
</task_context>

# Tarefa 9.0: Testes de Integração - Autenticação, Isolamento Multi-tenant e Agendamentos

## Visão Geral

Criar suite completa de testes de integração end-to-end usando WebApplicationFactory e Testcontainers para garantir que todo o fluxo de autenticação, isolamento multi-tenant e gestão de agendamentos funciona corretamente em ambiente real.

<requirements>
- Testes de integração para todos os fluxos críticos
- Uso de Testcontainers para PostgreSQL real
- WebApplicationFactory para API real
- Cenários de isolamento multi-tenant obrigatórios
- Testes de concorrência para validação de lock otimista
- Testes de autorização (401, 403)
- Cobertura de testes de integração > 70%
- Testes devem ser independentes e idempotentes
</requirements>

## Subtarefas

- [ ] 9.1 Configurar Testcontainers para PostgreSQL
- [ ] 9.2 Configurar WebApplicationFactory customizada
- [ ] 9.3 Criar fixtures e helpers para testes (factory de dados)
- [ ] 9.4 Criar testes de autenticação: cadastro e login (sucesso, falhas)
- [ ] 9.5 Criar testes de isolamento multi-tenant para barbeiros
- [ ] 9.6 Criar testes de isolamento multi-tenant para serviços
- [ ] 9.7 Criar testes de isolamento multi-tenant para agendamentos
- [ ] 9.8 Criar testes de criação de agendamento (sucesso, conflito, validações)
- [ ] 9.9 Criar testes de cancelamento de agendamento (sucesso, não permitido)
- [ ] 9.10 Criar testes de edição de agendamento (sucesso, conflito)
- [ ] 9.11 Criar testes de listagem de agendamentos (próximos, histórico)
- [ ] 9.12 Criar testes de concorrência (2 clientes, mesmo horário)
- [ ] 9.13 Criar testes de autorização (sem token, token expirado, recurso de outro cliente)
- [ ] 9.14 Configurar coleta de cobertura de testes de integração
- [ ] 9.15 Documentar padrões de teste de integração

## Detalhes de Implementação

### Setup - Testcontainers e WebApplicationFactory

```csharp
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remover DbContext existente
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Adicionar DbContext com Testcontainer
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Garantir que banco está criado
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        });
    }

    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("barbapp_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### Fixture e Helpers

```csharp
public class IntegrationTestBase : IClassFixture<IntegrationTestFactory>
{
    protected readonly HttpClient Client;
    protected readonly IntegrationTestFactory Factory;

    public IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> CadastrarClienteEObterToken(
        string codigoBarbearia = "TEST123",
        string nome = "João Silva",
        string telefone = "11987654321")
    {
        var request = new CadastrarClienteInput(codigoBarbearia, nome, telefone);
        var response = await Client.PostAsJsonAsync("/api/auth/cliente/cadastro", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CadastroClienteOutput>();
        return result!.Token;
    }

    protected async Task<Guid> CriarAgendamento(
        string token,
        Guid barbeiroId,
        List<Guid> servicosIds,
        DateTime dataHora)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var request = new CriarAgendamentoInput(barbeiroId, servicosIds, dataHora);
        var response = await Client.PostAsJsonAsync("/api/agendamentos", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<AgendamentoOutput>();
        return result!.Id;
    }

    protected async Task<List<BarbeiroDto>> ObterBarbeiros(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await Client.GetAsync("/api/barbeiros");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<BarbeiroDto>>() ?? new List<BarbeiroDto>();
    }
}
```

### Testes de Isolamento Multi-tenant

```csharp
public class MultiTenantIsolationTests : IntegrationTestBase
{
    public MultiTenantIsolationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Barbeiros_ClienteDeBarbeariaA_NaoVeBarbeirosDeBarbeariaB()
    {
        // Arrange: Criar 2 barbearias com dados diferentes
        var tokenBarbeariaA = await CadastrarClienteEObterToken("BARBA");
        var tokenBarbeariaB = await CadastrarClienteEObterToken("BARBB", "Maria", "11999999999");

        // Act: Cliente A busca barbeiros
        var barbeirosA = await ObterBarbeiros(tokenBarbeariaA);
        
        // Cliente B busca barbeiros
        var barbeirosB = await ObterBarbeiros(tokenBarbeariaB);

        // Assert: Listas devem ser diferentes e não ter sobreposição
        barbeirosA.Should().NotBeEmpty();
        barbeirosB.Should().NotBeEmpty();
        barbeirosA.Select(b => b.Id).Should().NotIntersectWith(barbeirosB.Select(b => b.Id));
    }

    [Fact]
    public async Task Agendamentos_ClienteDeBarbeariaA_NaoVeAgendamentosDeBarbeariaB()
    {
        // Arrange: Cliente A cria agendamento na barbearia A
        var tokenA = await CadastrarClienteEObterToken("BARBA");
        var barbeirosA = await ObterBarbeiros(tokenA);
        await CriarAgendamento(tokenA, barbeirosA[0].Id, new List<Guid> { Guid.NewGuid() }, DateTime.UtcNow.AddDays(1));

        // Cliente B consulta seus agendamentos na barbearia B
        var tokenB = await CadastrarClienteEObterToken("BARBB", "Maria", "11999999999");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var response = await Client.GetAsync("/api/agendamentos/meus");
        var agendamentosB = await response.Content.ReadFromJsonAsync<List<AgendamentoOutput>>();

        // Assert: Cliente B não deve ver agendamentos do cliente A
        agendamentosB.Should().BeEmpty();
    }

    [Fact]
    public async Task CriarAgendamento_BarbeiroDeOutraBarbearia_DeveRetornar403()
    {
        // Arrange: Cliente de barbearia A tenta agendar barbeiro de barbearia B
        var tokenA = await CadastrarClienteEObterToken("BARBA");
        var tokenB = await CadastrarClienteEObterToken("BARBB", "Maria", "11999999999");
        var barbeirosB = await ObterBarbeiros(tokenB);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var request = new CriarAgendamentoInput(barbeirosB[0].Id, new List<Guid> { Guid.NewGuid() }, DateTime.UtcNow.AddDays(1));

        // Act
        var response = await Client.PostAsJsonAsync("/api/agendamentos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

### Testes de Concorrência

```csharp
public class ConcorrenciaAgendamentosTests : IntegrationTestBase
{
    public ConcorrenciaAgendamentosTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task CriarAgendamento_DoisClientesSimultaneos_ApenasPrimeiroDeveSuceder()
    {
        // Arrange: 2 clientes diferentes, mesmo barbeiro, mesmo horário
        var token1 = await CadastrarClienteEObterToken("TEST123", "Cliente 1", "11111111111");
        var token2 = await CadastrarClienteEObterToken("TEST123", "Cliente 2", "22222222222");
        
        var barbeiros = await ObterBarbeiros(token1);
        var barbeiroId = barbeiros[0].Id;
        var dataHora = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var servicosIds = new List<Guid> { Guid.NewGuid() };

        var request = new CriarAgendamentoInput(barbeiroId, servicosIds, dataHora);

        // Act: Criar agendamentos simultaneamente
        var client1 = Factory.CreateClient();
        var client2 = Factory.CreateClient();
        
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var task1 = client1.PostAsJsonAsync("/api/agendamentos", request);
        var task2 = client2.PostAsJsonAsync("/api/agendamentos", request);

        var responses = await Task.WhenAll(task1, task2);

        // Assert: Apenas 1 deve suceder (201), outro deve falhar (422)
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.UnprocessableEntity);

        successCount.Should().Be(1);
        conflictCount.Should().Be(1);
    }
}
```

### Testes de Autorização

```csharp
public class AutorizacaoTests : IntegrationTestBase
{
    public AutorizacaoTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetBarbeiros_SemToken_DeveRetornar401()
    {
        // Act
        var response = await Client.GetAsync("/api/barbeiros");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelarAgendamento_AgendamentoDeOutroCliente_DeveRetornar403()
    {
        // Arrange: Cliente A cria agendamento
        var tokenA = await CadastrarClienteEObterToken("TEST123", "Cliente A", "11111111111");
        var barbeiros = await ObterBarbeiros(tokenA);
        var agendamentoId = await CriarAgendamento(
            tokenA, 
            barbeiros[0].Id, 
            new List<Guid> { Guid.NewGuid() }, 
            DateTime.UtcNow.AddDays(1));

        // Cliente B tenta cancelar
        var tokenB = await CadastrarClienteEObterToken("TEST123", "Cliente B", "22222222222");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act
        var response = await Client.DeleteAsync($"/api/agendamentos/{agendamentoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

## Critérios de Sucesso

- ✅ Testcontainers configurado e funcionando
- ✅ Todos os testes de isolamento multi-tenant passando
- ✅ Testes de concorrência validando lock otimista
- ✅ Testes de autorização cobrindo 401 e 403
- ✅ Testes independentes (podem executar em qualquer ordem)
- ✅ Cobertura de testes de integração > 70%
- ✅ Testes executam em < 30 segundos total
- ✅ Documentação de padrões de teste criada
- ✅ CI/CD configurado para executar testes automaticamente
