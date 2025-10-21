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
public class LandingPagesControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _dbFixture;
    private static bool _dbInitialized;
    private static readonly object _initLock = new();

    public LandingPagesControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;

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
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<BarbAppDbContext>(options =>
                        options.UseNpgsql(_dbFixture.ConnectionString));

                    services.AddScoped<BarbApp.Application.Interfaces.IEmailService, NoOpEmailService>();
                });

                builder.UseEnvironment("Testing");
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetConfig_WithValidBarbershopId_ShouldReturn200AndConfig()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        // Act
        var response = await client.GetAsync($"/api/admin/landing-pages/{barbershopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<LandingPageConfigOutput>();
        config.Should().NotBeNull();
        config!.BarbershopId.Should().Be(barbershopId);
        config.TemplateId.Should().Be(1);
        config.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetConfig_WithoutAuthorization_ShouldReturn401()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/admin/landing-pages/{barbershopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConfig_WrongBarbershop_ShouldReturn403()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var anotherBarbershopId = Guid.NewGuid();
        var client = CreateAuthorizedClient(anotherBarbershopId);

        // Act
        var response = await client.GetAsync($"/api/admin/landing-pages/{barbershopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetConfig_NonExistentLandingPage_ShouldReturn404()
    {
        // Arrange
        var barbershopId = await CreateBarbershopWithoutLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        // Act
        var response = await client.GetAsync($"/api/admin/landing-pages/{barbershopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateConfig_WithValidData_ShouldReturn204()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var updateInput = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Nova descrição da barbearia",
            OpeningHours: "Seg-Sex: 10:00-20:00",
            InstagramUrl: "https://instagram.com/barbershop",
            FacebookUrl: "https://facebook.com/barbershop",
            WhatsappNumber: "+5511987654321",
            Services: null
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/admin/landing-pages/{barbershopId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await client.GetAsync($"/api/admin/landing-pages/{barbershopId}");
        var config = await getResponse.Content.ReadFromJsonAsync<LandingPageConfigOutput>();
        config.Should().NotBeNull();
        config!.TemplateId.Should().Be(2);
        config.AboutText.Should().Be("Nova descrição da barbearia");
        config.OpeningHours.Should().Be("Seg-Sex: 10:00-20:00");
        config.InstagramUrl.Should().Be("https://instagram.com/barbershop");
    }

    [Fact]
    public async Task UpdateConfig_WithServices_ShouldReturn204AndUpdateServices()
    {
        // Arrange
        var (barbershopId, serviceIds) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var updateInput = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: new List<ServiceDisplayInput>
            {
                new(serviceIds[0], 1, true),
                new(serviceIds[1], 2, false)
            }
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/admin/landing-pages/{barbershopId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify services
        var getResponse = await client.GetAsync($"/api/admin/landing-pages/{barbershopId}");
        var config = await getResponse.Content.ReadFromJsonAsync<LandingPageConfigOutput>();
        config.Should().NotBeNull();
        config!.Services.Should().HaveCount(2);
        config.Services[0].IsVisible.Should().BeTrue();
        config.Services[1].IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateConfig_WithoutAuthorization_ShouldReturn401()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var updateInput = new UpdateLandingPageInput(null, null, null, null, null, null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/landing-pages/{barbershopId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateConfig_WrongBarbershop_ShouldReturn403()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var anotherBarbershopId = Guid.NewGuid();
        var client = CreateAuthorizedClient(anotherBarbershopId);

        var updateInput = new UpdateLandingPageInput(null, null, "Test", null, null, null, null, null);

        // Act
        var response = await client.PutAsJsonAsync($"/api/admin/landing-pages/{barbershopId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateConfig_NonExistentLandingPage_ShouldReturn404()
    {
        // Arrange
        var barbershopId = await CreateBarbershopWithoutLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var updateInput = new UpdateLandingPageInput(null, null, "Test", null, null, null, null, null);

        // Act
        var response = await client.PutAsJsonAsync($"/api/admin/landing-pages/{barbershopId}", updateInput);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadLogo_WithValidFile_ShouldReturn200AndLogoUrl()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[100]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        // Act
        var response = await client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("logoUrl");
        result.Should().ContainKey("message");
    }

    [Fact]
    public async Task UploadLogo_WithoutFile_ShouldReturn400()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var content = new MultipartFormDataContent();

        // Act
        var response = await client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_WithInvalidFileType_ShouldReturn400()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[100]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        // Act
        var response = await client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_WithFileTooLarge_ShouldReturn400()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var client = CreateAuthorizedClient(barbershopId);

        var content = new MultipartFormDataContent();
        var largeFile = new byte[3 * 1024 * 1024]; // 3MB
        var fileContent = new ByteArrayContent(largeFile);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "large-logo.png");

        // Act
        var response = await client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_WithoutAuthorization_ShouldReturn401()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadLogo_WrongBarbershop_ShouldReturn403()
    {
        // Arrange
        var (barbershopId, _) = await CreateBarbershopWithLandingPageAsync();
        var anotherBarbershopId = Guid.NewGuid();
        var client = CreateAuthorizedClient(anotherBarbershopId);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[100]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        // Act
        var response = await client.PostAsync($"/api/admin/landing-pages/{barbershopId}/logo", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateAuthorizedClient(Guid barbershopId)
    {
        var client = _factory.CreateClient();
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminBarbearia",
            email: "admin@barbearia.com",
            barbeariaId: barbershopId
        );
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateRandomUniqueCode()
    {
        // Valid characters: A-Z (excluding I, O) and 2-9 (excluding 0, 1)
        const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var code = new char[8];
        
        for (int i = 0; i < 8; i++)
        {
            code[i] = validChars[random.Next(validChars.Length)];
        }
        
        return new string(code);
    }

    private static string GenerateRandomCNPJ()
    {
        var random = new Random();
        
        // Generate base 12 digits
        var cnpj = new int[14];
        for (int i = 0; i < 12; i++)
        {
            cnpj[i] = random.Next(0, 10);
        }
        
        // Calculate first verification digit
        int sum = 0;
        int[] weight1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (int i = 0; i < 12; i++)
        {
            sum += cnpj[i] * weight1[i];
        }
        int remainder = sum % 11;
        cnpj[12] = remainder < 2 ? 0 : 11 - remainder;
        
        // Calculate second verification digit
        sum = 0;
        int[] weight2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        for (int i = 0; i < 13; i++)
        {
            sum += cnpj[i] * weight2[i];
        }
        remainder = sum % 11;
        cnpj[13] = remainder < 2 ? 0 : 11 - remainder;
        
        return string.Join("", cnpj);
    }

    private async Task<(Guid barbershopId, List<Guid> serviceIds)> CreateBarbershopWithLandingPageAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var address = BarbApp.Domain.Entities.Address.Create(
            "01310-100",
            "Av. Paulista",
            "1000",
            "Sala 10",
            "Bela Vista",
            "São Paulo",
            "SP"
        );

        var document = BarbApp.Domain.ValueObjects.Document.Create(GenerateRandomCNPJ());
        var code = BarbApp.Domain.ValueObjects.UniqueCode.Create(GenerateRandomUniqueCode());

        var barbershop = BarbApp.Domain.Entities.Barbershop.Create(
            name: $"Barbearia Teste {Guid.NewGuid()}",
            document: document,
            phone: "(11) 98765-4321",
            ownerName: "João Teste",
            email: $"teste{Guid.NewGuid()}@teste.com",
            address: address,
            code: code,
            createdBy: "integration-test"
        );

        context.Barbershops.Add(barbershop);
        await context.SaveChangesAsync();

        var service1 = BarbApp.Domain.Entities.BarbershopService.Create(
            barbershop.Id,
            "Corte Tradicional",
            "Corte clássico",
            30,
            50.00m
        );

        var service2 = BarbApp.Domain.Entities.BarbershopService.Create(
            barbershop.Id,
            "Barba",
            "Barba completa",
            20,
            30.00m
        );

        context.BarbershopServices.AddRange(service1, service2);
        await context.SaveChangesAsync();

        var landingPageConfig = BarbApp.Domain.Entities.LandingPageConfig.Create(
            barbershop.Id,
            1,
            barbershop.Phone ?? "",
            "Seg-Sex: 09:00-18:00"
        );

        context.LandingPageConfigs.Add(landingPageConfig);
        await context.SaveChangesAsync();

        var landingPageService1 = BarbApp.Domain.Entities.LandingPageService.Create(
            landingPageConfig.Id,
            service1.Id,
            1,
            true
        );

        var landingPageService2 = BarbApp.Domain.Entities.LandingPageService.Create(
            landingPageConfig.Id,
            service2.Id,
            2,
            true
        );

        context.LandingPageServices.AddRange(landingPageService1, landingPageService2);
        await context.SaveChangesAsync();

        return (barbershop.Id, new List<Guid> { service1.Id, service2.Id });
    }

    private async Task<Guid> CreateBarbershopWithoutLandingPageAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var address = BarbApp.Domain.Entities.Address.Create(
            "01310-100",
            "Av. Paulista",
            "2000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP"
        );

        var document = BarbApp.Domain.ValueObjects.Document.Create(GenerateRandomCNPJ());
        var code = BarbApp.Domain.ValueObjects.UniqueCode.Create(GenerateRandomUniqueCode());

        var barbershop = BarbApp.Domain.Entities.Barbershop.Create(
            name: $"Barbearia Sem Landing {Guid.NewGuid()}",
            document: document,
            phone: "(11) 91234-5678",
            ownerName: "Maria Teste",
            email: $"maria{Guid.NewGuid()}@teste.com",
            address: address,
            code: code,
            createdBy: "integration-test"
        );

        context.Barbershops.Add(barbershop);
        await context.SaveChangesAsync();

        return barbershop.Id;
    }
}
