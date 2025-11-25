using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BarbApp.IntegrationTests;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class LandingPagesControllerIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private Guid _barbeariaId1;
    private Guid _barbeariaId2;
    private Guid _serviceId1;

    public LandingPagesControllerIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = dbFixture.CreateFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.ResetDatabaseAsync();
        _factory.EnsureDatabaseInitialized();
        await SetupTestData();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SetupTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var landingPageService = scope.ServiceProvider.GetRequiredService<BarbApp.Application.Interfaces.UseCases.ILandingPageService>();

        // Create two barbearias for testing
        var address1 = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var document1 = Document.Create("11111111000191");
        var code1 = UniqueCode.Create("LANDING2");
        var barbearia1 = Barbershop.Create(
            "Barbearia Teste 1",
            document1,
            "(11) 98765-4321",
            "João Silva",
            "joao@landing1.com",
            address1,
            code1,
            "Password123!"
        );
        dbContext.Addresses.Add(address1);
        dbContext.Barbershops.Add(barbearia1);

        var address2 = Address.Create("01310200", "Av. Brigadeiro", "2000", null, "Bela Vista", "São Paulo", "SP");
        var document2 = Document.Create("22222222000192");
        var code2 = UniqueCode.Create("LANDING3");
        var barbearia2 = Barbershop.Create(
            "Barbearia Teste 2",
            document2,
            "(11) 98765-4322",
            "Maria Santos",
            "maria@landing2.com",
            address2,
            code2,
            "Password123!"
        );
        dbContext.Addresses.Add(address2);
        dbContext.Barbershops.Add(barbearia2);

        await dbContext.SaveChangesAsync();

        _barbeariaId1 = barbearia1.Id;
        _barbeariaId2 = barbearia2.Id;

        // Create a test service for barbearia1
        var service = BarbApp.Domain.Entities.BarbershopService.Create(
            _barbeariaId1,
            "Corte Masculino",
            "Corte de cabelo masculino completo",
            30,
            25.00m);
        dbContext.BarbershopServices.Add(service);
        await dbContext.SaveChangesAsync();
        _serviceId1 = service.Id;

        // Create landing pages for the test barbearias
        await landingPageService.CreateAsync(_barbeariaId1);
        await landingPageService.CreateAsync(_barbeariaId2);
    }

    [Fact]
    public async Task GetConfig_AdminBarbeariaToken_ValidBarbershopId_ShouldReturn200AndConfig()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<LandingPageConfigOutput>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        config.Should().NotBeNull();
        config!.BarbershopId.Should().Be(_barbeariaId1);
    }

    [Fact]
    public async Task GetConfig_AdminCentralToken_ValidBarbershopId_ShouldReturn200AndConfig()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<LandingPageConfigOutput>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        config.Should().NotBeNull();
        config!.BarbershopId.Should().Be(_barbeariaId1);
    }

    [Fact]
    public async Task GetConfig_AdminBarbeariaToken_WrongBarbershopId_ShouldReturn403()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/admin/landing-pages/{_barbeariaId2}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetConfig_NoAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync($"/api/admin/landing-pages/{_barbeariaId1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConfig_InvalidBarbershopId_ShouldReturn404()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");
        var invalidId = Guid.NewGuid();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/admin/landing-pages/{invalidId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateConfig_AdminBarbeariaToken_ValidInput_ShouldReturn204()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Fri 9AM-6PM",
            InstagramUrl: "https://instagram.com/barber",
            FacebookUrl: "https://facebook.com/barber",
            WhatsappNumber: "+5511987654321",
            Services: new List<ServiceDisplayInput>
            {
                new ServiceDisplayInput(_serviceId1, 1, true)
            }
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateConfig_AdminCentralToken_ValidInput_ShouldReturn204()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Fri 9AM-6PM",
            InstagramUrl: "https://instagram.com/barber",
            FacebookUrl: "https://facebook.com/barber",
            WhatsappNumber: "+5511987654321",
            Services: new List<ServiceDisplayInput>
            {
                new ServiceDisplayInput(_serviceId1, 1, true)
            }
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateConfig_AdminBarbeariaToken_WrongBarbershopId_ShouldReturn403()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Fri 9AM-6PM",
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{_barbeariaId2}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateConfig_NoAuthentication_ShouldReturn401()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Fri 9AM-6PM",
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateConfig_InvalidBarbershopId_ShouldReturn404()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");
        var invalidId = Guid.NewGuid();
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Fri 9AM-6PM",
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{invalidId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateConfig_InvalidInput_ShouldReturn400()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        var invalidInput = new
        {
            templateId = 10, // Invalid: should be between 1-5
            whatsappNumber = "invalid-phone", // Invalid: should match +55XXXXXXXXXXX format
            logoUrl = "not-a-url" // Invalid: should be a valid URL
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/landing-pages/{_barbeariaId1}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(invalidInput), Encoding.UTF8, "application/json");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_AdminBarbeariaToken_ValidFile_ShouldReturn200()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        // Minimal valid PNG file (1x1 transparent pixel)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "File", "test-logo.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{_barbeariaId1}/logo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("logoUrl");
        content.Should().Contain("message");
    }

    [Fact]
    public async Task UploadLogo_AdminCentralToken_ValidFile_ShouldReturn200()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");
        // Minimal valid PNG file (1x1 transparent pixel)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "File", "test-logo.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{_barbeariaId1}/logo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("logoUrl");
        content.Should().Contain("message");
    }

    [Fact]
    public async Task UploadLogo_AdminBarbeariaToken_WrongBarbershopId_ShouldReturn403()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        // Minimal valid PNG file (1x1 transparent pixel)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "File", "test-logo.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{_barbeariaId2}/logo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadLogo_NoAuthentication_ShouldReturn401()
    {
        // Arrange
        // Minimal valid PNG file (1x1 transparent pixel)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "File", "test-logo.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{_barbeariaId1}/logo");
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadLogo_NoFile_ShouldReturn400()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminBarbearia", barbeariaId: _barbeariaId1);
        var multipartContent = new MultipartFormDataContent();
        // Add an empty file field
        var emptyContent = new ByteArrayContent(new byte[0]);
        emptyContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        multipartContent.Add(emptyContent, "File", "empty.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{_barbeariaId1}/logo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Nenhum arquivo foi enviado");
    }

    [Fact]
    public async Task UploadLogo_InvalidBarbershopId_ShouldReturn404()
    {
        // Arrange
        var token = IntegrationTestWebAppFactory.GenerateTestJwtToken("test-user", "AdminCentral");
        var invalidId = Guid.NewGuid();
        // Minimal valid PNG file (1x1 transparent pixel)
        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "File", "test-logo.png");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/landing-pages/{invalidId}/logo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = multipartContent;
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
