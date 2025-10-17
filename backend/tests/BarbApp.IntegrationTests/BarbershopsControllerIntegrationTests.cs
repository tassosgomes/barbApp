using System.Net;
using System.Net.Http.Json;
using BarbApp.Application.DTOs;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class BarbershopsControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _dbFixture;
    private static bool _dbInitialized;
    private static readonly object _initLock = new();

    public BarbershopsControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;

        // Initialize database once
        if (!_dbInitialized)
        {
            lock (_initLock)
            {
                if (!_dbInitialized)
                {
                    _dbFixture.RunMigrations();
                    _dbInitialized = true;
                }
            }
        }

        // Create factory with inline configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
                        ["JwtSettings:Issuer"] = "BarbApp-Test",
                        ["JwtSettings:Audience"] = "BarbApp-Test-Users",
                        ["JwtSettings:ExpirationMinutes"] = "60"
                    }!);
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add test DbContext
                    services.AddDbContext<BarbAppDbContext>(options =>
                        options.UseNpgsql(_dbFixture.ConnectionString));

                    // Register NoOpEmailService for testing (needed for Task 15.3)
                    services.AddScoped<BarbApp.Application.Interfaces.IEmailService, NoOpEmailService>();
                });

                builder.UseEnvironment("Testing");
            });

        _client = _factory.CreateClient();

        // Set AdminCentral JWT token
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminCentral",
            email: "admin@test.com",
            barbeariaId: null
        );
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateBarbershop_ValidData_ShouldReturn201AndCreateBarbershop()
    {
        // Arrange
        var input = new
        {
            name = "Barbearia Teste Integração",
            document = "12345678000195",
            phone = "(11) 98765-4321",
            ownerName = "João Silva",
            email = "joao@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1000",
            complement = "Sala 15",
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbearias", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BarbershopOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Barbearia Teste Integração");
        result.Document.Should().Be("12345678000195");
        result.Phone.Should().Be("11987654321");
        result.OwnerName.Should().Be("João Silva");
        result.Email.Should().Be("joao@teste.com");
        result.Code.Should().NotBeNullOrEmpty();
        result.Code.Length.Should().Be(8);
        result.IsActive.Should().BeTrue();
        result.Address.Should().NotBeNull();
        result.Address.ZipCode.Should().Be("01310100");
        result.Address.Street.Should().Be("Av. Paulista");
        result.Address.Number.Should().Be("1000");
        result.Address.Complement.Should().Be("Sala 15");
        result.Address.Neighborhood.Should().Be("Bela Vista");
        result.Address.City.Should().Be("São Paulo");
        result.Address.State.Should().Be("SP");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateBarbershop_DuplicateDocument_ShouldReturn422()
    {
        // Arrange - Create first barbershop
        var input1 = new
        {
            name = "Barbearia Original",
            document = "98765432000189",
            phone = "(11) 98765-4321",
            ownerName = "João Silva",
            email = "joao1@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1000",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var response1 = await _client.PostAsJsonAsync("/api/barbearias", input1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create second barbershop with same document
        var input2 = new
        {
            name = "Barbearia Duplicada",
            document = "98765432000189", // Same document
            phone = "(11) 98765-4322",
            ownerName = "Maria Silva",
            email = "maria@teste.com",
            zipCode = "01310-101",
            street = "Av. Paulista",
            number = "1001",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var response2 = await _client.PostAsJsonAsync("/api/barbearias", input2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetBarbershop_ExistingId_ShouldReturn200AndBarbershop()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia para Get",
            document = "11223344000187",
            phone = "(11) 98765-4323",
            ownerName = "Carlos Silva",
            email = "carlos@teste.com",
            zipCode = "01310-102",
            street = "Av. Paulista",
            number = "1002",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarbershop = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        createdBarbershop.Should().NotBeNull();

        // Act
        var getResponse = await _client.GetAsync($"/api/barbearias/{createdBarbershop!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarbershop.Id);
        result.Name.Should().Be("Barbearia para Get");
        result.Document.Should().Be("11223344000187");
    }

    [Fact]
    public async Task GetBarbershop_NonExistingId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/api/barbearias/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListBarbershops_NoFilters_ShouldReturnPaginatedResults()
    {
        // Arrange - Create multiple barbershops
        var documents = new[] { "55667788000174", "99887766000153", "44332211000196", "77889955000142", "22558899000108" };
        for (int i = 0; i < 5; i++)
        {
            var input = new
            {
                name = $"Barbearia List {i}",
                document = documents[i],
                phone = $"(11) 98765-432{i}",
                ownerName = $"Proprietário {i}",
                email = $"list{i}@teste.com",
                zipCode = "01310-100",
                street = "Av. Paulista",
                number = $"10{i:00}",
                complement = (string?)null,
                neighborhood = "Bela Vista",
                city = "São Paulo",
                state = "SP"
            };

            var response = await _client.PostAsJsonAsync("/api/barbearias", input);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act
        var listResponse = await _client.GetAsync("/api/barbearias?page=1&pageSize=10");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbershopsOutput>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().AllSatisfy(item =>
        {
            item.Id.Should().NotBeEmpty();
            item.Name.Should().NotBeNullOrEmpty();
            item.Code.Should().NotBeNullOrEmpty();
            item.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task ListBarbershops_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange - Create barbershops with different names
        var input1 = new
        {
            name = "Barbearia Premium",
            document = "33445566000172",
            phone = "(11) 98765-4325",
            ownerName = "Premium Owner",
            email = "premium@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1005",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var input2 = new
        {
            name = "Barbearia Standard",
            document = "66778899000138",
            phone = "(11) 98765-4326",
            ownerName = "Standard Owner",
            email = "standard@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1006",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var response1 = await _client.PostAsJsonAsync("/api/barbearias", input1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        var response2 = await _client.PostAsJsonAsync("/api/barbearias", input2);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Search for "Premium"
        var searchResponse = await _client.GetAsync("/api/barbearias?searchTerm=Premium&page=1&pageSize=10");

        // Assert
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await searchResponse.Content.ReadFromJsonAsync<PaginatedBarbershopsOutput>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Barbearia Premium");
    }

    [Fact]
    public async Task UpdateBarbershop_ExistingId_ShouldReturn200AndUpdateBarbershop()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia para Update",
            document = "88990011000125",
            phone = "(11) 98765-4327",
            ownerName = "Update Owner",
            email = "update@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1007",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarbershop = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        createdBarbershop.Should().NotBeNull();

        var updateInput = new
        {
            id = createdBarbershop!.Id,
            name = "Barbearia Atualizada",
            phone = "(11) 98765-4328",
            ownerName = "Updated Owner",
            email = "updated@teste.com",
            zipCode = "01310-101",
            street = "Av. Brigadeiro",
            number = "1008",
            complement = "Apto 10",
            neighborhood = "Jardim Paulista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/barbearias/{createdBarbershop.Id}", updateInput);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await updateResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarbershop.Id);
        result.Name.Should().Be("Barbearia Atualizada");
        result.Phone.Should().Be("11987654328");
        result.OwnerName.Should().Be("Updated Owner");
        result.Email.Should().Be("updated@teste.com");
        result.Address.ZipCode.Should().Be("01310101");
        result.Address.Street.Should().Be("Av. Brigadeiro");
        result.Address.Number.Should().Be("1008");
        result.Address.Complement.Should().Be("Apto 10");
        result.Address.Neighborhood.Should().Be("Jardim Paulista");
        result.Document.Should().Be("88990011000125"); // Should not change
        result.Code.Should().Be(createdBarbershop.Code); // Should not change
    }

    [Fact]
    public async Task UpdateBarbershop_NonExistingId_ShouldReturn404()
    {
        // Arrange
        var updateInput = new
        {
            id = Guid.NewGuid(),
            name = "Barbearia Inexistente",
            phone = "(11) 98765-4329",
            ownerName = "Inexistent Owner",
            email = "inexistent@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1009",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/barbearias/{updateInput.id}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBarbershop_ExistingId_ShouldReturn204AndSoftDelete()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia para Delete",
            document = "00112233000141",
            phone = "(11) 98765-4330",
            ownerName = "Delete Owner",
            email = "delete@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1010",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBarbershop = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        createdBarbershop.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/barbearias/{createdBarbershop!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's soft deleted (not returned in list by default)
        var listResponse = await _client.GetAsync("/api/barbearias?page=1&pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.Content.ReadFromJsonAsync<PaginatedBarbershopsOutput>();
        listResult.Should().NotBeNull();
        listResult!.Items.Should().NotContain(item => item.Id == createdBarbershop.Id);
    }

    [Fact]
    public async Task DeleteBarbershop_NonExistingId_ShouldReturn404()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/barbearias/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBarbershop_InvalidData_ShouldReturn400()
    {
        // Arrange
        var invalidInput = new
        {
            name = "", // Invalid: empty name
            document = "invalid-document",
            phone = "invalid-phone",
            ownerName = "João Silva",
            email = "invalid-email",
            zipCode = "01310100", // Invalid format
            street = "Av. Paulista",
            number = "1000",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbearias", invalidInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AccessBarbershops_WithoutAuth_ShouldReturn401()
    {
        // Arrange - Create client without auth
        var unauthClient = _factory.CreateClient();

        var input = new
        {
            name = "Barbearia Sem Auth",
            document = "00224466000107",
            phone = "(11) 98765-4331",
            ownerName = "No Auth Owner",
            email = "noauth@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "1011",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        // Act
        var response = await unauthClient.PostAsJsonAsync("/api/barbearias", input);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendCredentials_ValidBarbershop_ShouldReturn200AndResendEmail()
    {
        // Arrange - Create barbershop first
        var createInput = new
        {
            name = "Barbearia Reenvio Teste",
            document = "11223344000156",
            phone = "(11) 98765-4322",
            ownerName = "Maria Santos",
            email = "maria@reenvio.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "2000",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdBarbershop = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        createdBarbershop.Should().NotBeNull();

        // Act
        var response = await _client.PostAsync($"/api/barbearias/{createdBarbershop!.Id}/reenviar-credenciais", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Credenciais reenviadas com sucesso");
    }

    [Fact]
    public async Task ResendCredentials_NonExistentBarbershop_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/barbearias/{nonExistentId}/reenviar-credenciais", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendCredentials_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var unauthClient = _factory.CreateClient();
        var barbershopId = Guid.NewGuid();

        // Act
        var response = await unauthClient.PostAsync($"/api/barbearias/{barbershopId}/reenviar-credenciais", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendCredentials_WithNonAdminCentralRole_ShouldReturn403()
    {
        // Arrange - Create client with AdminBarbearia token (not AdminCentral)
        var adminBarbeariaClient = _factory.CreateClient();
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "adminbarb@test.com",
            barbeariaId: Guid.NewGuid()
        );
        adminBarbeariaClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var barbershopId = Guid.NewGuid();

        // Act
        var response = await adminBarbeariaClient.PostAsync($"/api/barbearias/{barbershopId}/reenviar-credenciais", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #region GetByCode Tests (Public Endpoint)

    [Fact]
    public async Task GetByCode_ValidCode_ShouldReturn200WithBasicData()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia para Validação de Código",
            document = "55667788000199",
            phone = "(11) 98765-9999",
            ownerName = "Tasso Gomes",
            email = "tasso.validacao@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "9999",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        var codigo = created!.Code;

        // Create unauthenticated client (endpoint is public)
        var publicClient = _factory.CreateClient();

        // Act
        var response = await publicClient.GetAsync($"/api/barbearias/codigo/{codigo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidateBarbeariaCodeResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Nome.Should().Be("Barbearia para Validação de Código");
        result.Codigo.Should().Be(codigo);
        result.IsActive.Should().BeTrue();

        // Verify no sensitive data is exposed
        var jsonResponse = await response.Content.ReadAsStringAsync();
        jsonResponse.Should().NotContain("document");
        jsonResponse.Should().NotContain("phone");
        jsonResponse.Should().NotContain("email");
        jsonResponse.Should().NotContain("address");
        jsonResponse.Should().NotContain("ownerName");
    }

    [Fact]
    public async Task GetByCode_InvalidCode_ShouldReturn404()
    {
        // Arrange
        var invalidCode = "INVALID1"; // Código que não existe

        // Create unauthenticated client (endpoint is public)
        var publicClient = _factory.CreateClient();

        // Act
        var response = await publicClient.GetAsync($"/api/barbearias/codigo/{invalidCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCode_InvalidFormat_ShouldReturn400()
    {
        // Arrange
        var invalidFormat = "ABC"; // Menos de 8 caracteres

        // Create unauthenticated client (endpoint is public)
        var publicClient = _factory.CreateClient();

        // Act
        var response = await publicClient.GetAsync($"/api/barbearias/codigo/{invalidFormat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCode_InactiveBarbershop_ShouldReturn403()
    {
        // Arrange - Create a barbershop and deactivate it
        var createInput = new
        {
            name = "Barbearia Inativa",
            document = "99887766000155",
            phone = "(11) 98765-8888",
            ownerName = "Pedro Silva",
            email = "pedro.inativa@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "8888",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        var codigo = created!.Code;

        // Deactivate the barbershop
        var deactivateResponse = await _client.PutAsync($"/api/barbearias/{created.Id}/desativar", null);
        deactivateResponse.EnsureSuccessStatusCode();

        // Create unauthenticated client (endpoint is public)
        var publicClient = _factory.CreateClient();

        // Act
        var response = await publicClient.GetAsync($"/api/barbearias/codigo/{codigo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetByCode_NoAuthRequired_ShouldAllowAnonymousAccess()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia Pública",
            document = "44556677000188",
            phone = "(11) 98765-7777",
            ownerName = "Ana Silva",
            email = "ana.publica@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "7777",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        var codigo = created!.Code;

        // Create completely fresh client with no authentication
        var anonymousClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost")
        };
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _dbFixture.ConnectionString
                    });
                });

                builder.UseEnvironment("Testing");
            });

        anonymousClient = factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync($"/api/barbearias/codigo/{codigo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyBarbershop_AsAdminBarbearia_ShouldReturn200WithBarbershopData()
    {
        // Arrange - Create a barbershop first
        var createInput = new
        {
            name = "Barbearia Minha",
            document = "11223344000177",
            phone = "(11) 98765-9999",
            ownerName = "Carlos Silva",
            email = "carlos.minha@teste.com",
            zipCode = "01310-100",
            street = "Av. Paulista",
            number = "9999",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", createInput);
        createResponse.EnsureSuccessStatusCode();
        var createdBarbershop = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();

        // Create a client with AdminBarbearia token
        var adminBarbeariaClient = _factory.CreateClient();
        var adminBarbeariaToken = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin.barbearia@test.com",
            barbeariaId: createdBarbershop!.Id
        );
        adminBarbeariaClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminBarbeariaToken);

        // Act
        var response = await adminBarbeariaClient.GetAsync("/api/barbearias/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarbershopOutput>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBarbershop.Id);
        result.Name.Should().Be("Barbearia Minha");
        result.Code.Should().Be(createdBarbershop.Code);
    }

    [Fact]
    public async Task GetMyBarbershop_AsAdminCentral_ShouldReturn403()
    {
        // Arrange - Use the existing AdminCentral client (no barbeariaId in token)

        // Act
        var response = await _client.GetAsync("/api/barbearias/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyBarbershop_Unauthenticated_ShouldReturn401()
    {
        // Arrange - Create unauthenticated client
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/barbearias/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}