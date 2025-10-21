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
public class PublicLandingPageControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _dbFixture;
    private static bool _dbInitialized;
    private static readonly object _initLock = new();

    public PublicLandingPageControllerIntegrationTests(DatabaseFixture dbFixture)
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
    public async Task GetPublicLandingPage_WithValidCode_ShouldReturn200AndData()
    {
        // Arrange
        var (code, barbershopId) = await CreateBarbershopWithLandingPageAsync();

        // Act
        var response = await _client.GetAsync($"/api/public/barbershops/{code}/landing-page");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<PublicLandingPageOutput>();
        data.Should().NotBeNull();
        data!.Barbershop.Id.Should().Be(barbershopId);
        data.Barbershop.Name.Should().Be("Barbearia Teste");
        data.LandingPage.TemplateId.Should().Be(1);
        data.LandingPage.Services.Should().HaveCount(2);
        data.LandingPage.WhatsappNumber.Should().Be("(11) 98765-4321");
    }

    [Fact]
    public async Task GetPublicLandingPage_WithInvalidCode_ShouldReturn404()
    {
        // Arrange
        var invalidCode = "INVALID";

        // Act
        var response = await _client.GetAsync($"/api/public/barbershops/{invalidCode}/landing-page");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        error.Should().NotBeNull();
        error!.Should().ContainKey("error");
        error["error"].Should().Be("Landing page não encontrada");
    }

    [Fact]
    public async Task GetPublicLandingPage_WithBarbershopWithoutLandingPage_ShouldReturn404()
    {
        // Arrange
        var code = await CreateBarbershopWithoutLandingPageAsync();

        // Act
        var response = await _client.GetAsync($"/api/public/barbershops/{code}/landing-page");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        error.Should().NotBeNull();
        error!.Should().ContainKey("error");
        error["error"].Should().Be("Landing page não encontrada");
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

    private async Task<(string code, Guid barbershopId)> CreateBarbershopWithLandingPageAsync()
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
            name: "Barbearia Teste",
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

        return (code.Value, barbershop.Id);
    }

    private async Task<string> CreateBarbershopWithoutLandingPageAsync()
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
            name: "Barbearia Sem Landing",
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

        return code.Value;
    }
}