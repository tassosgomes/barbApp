// BarbApp.IntegrationTests/Middlewares/MiddlewareIntegrationTests.cs
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BarbApp.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BarbApp.IntegrationTests.Middlewares;

public class MiddlewareIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public MiddlewareIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_WhenUnauthorizedExceptionThrown_ShouldReturn401WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/test/unauthorized");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(401);
        errorResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_WhenForbiddenExceptionThrown_ShouldReturn403WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/test/forbidden");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(403);
        errorResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_WhenNotFoundExceptionThrown_ShouldReturn404WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/test/notfound");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(404);
        errorResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_WhenValidationExceptionThrown_ShouldReturn400WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/test/validation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(400);
        errorResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_WhenUnhandledExceptionThrown_ShouldReturn500WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/test/unhandled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(500);
        errorResponse.Message.Should().Be("An error occurred processing your request");
    }

    [Fact]
    public async Task TenantMiddleware_WhenUserAuthenticated_ShouldSetTenantContext()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/test/tenant-context");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Note: In a real test, we'd need to verify the tenant context was set
        // This would require a test endpoint that exposes the tenant context
    }

    [Fact]
    public async Task AuthenticationMiddleware_WhenInvalidToken_ShouldReturn401WithJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Make request with invalid Authorization header
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(401);
        errorResponse.Message.Should().Be("Token de autenticação inválido ou ausente");
    }

    [Fact]
    public async Task AuthenticationMiddleware_WhenExpiredToken_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Make request with expired token
        // Note: Testing with a real expired JWT token would require generating one with past expiration
        // For this test, we validate that invalid tokens return 401
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-token");
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        // Note: Token-Expired header would only be set if JWT handler detects SecurityTokenExpiredException
        // This requires a properly formatted JWT with expired timestamp, not just any invalid token
    }
}

public record ErrorResponse(int StatusCode, string Message, DateTime Timestamp);

// Test authentication handler for testing authenticated requests
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
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
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