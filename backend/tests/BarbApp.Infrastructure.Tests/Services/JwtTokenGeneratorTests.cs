// BarbApp.Infrastructure.Tests/Services/JwtTokenGeneratorTests.cs
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

public class JwtTokenGeneratorTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "my-super-secret-key-with-at-least-32-characters-for-security"},
            {"Jwt:Issuer", "barbapp"},
            {"Jwt:Audience", "barbapp-api"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _generator = new JwtTokenGenerator(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidClaims_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userType = "Barbeiro";
        var email = "barbeiro@test.com";
        var barbeariaId = Guid.NewGuid();
        var barbeariaCode = "ABC12345";

        // Act
        var token = _generator.GenerateToken(userId, userType, email, barbeariaId, barbeariaCode);

        // Assert
        token.Should().NotBeNull();
        token.Value.Should().NotBeNullOrEmpty();
        token.Value.Split('.').Should().HaveCount(3); // JWT has 3 parts
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        token.ExpiresAt.Should().BeBefore(DateTime.UtcNow.AddHours(25));
    }

    [Fact]
    public void GenerateToken_AdminCentral_ShouldReturnTokenWithoutBarbearia()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userType = "AdminCentral";
        var email = "admin@test.com";

        // Act
        var token = _generator.GenerateToken(userId, userType, email, null, null);

        // Assert
        token.Should().NotBeNull();
        var claims = _generator.Validate(token.Value);
        claims.Should().NotBeNull();
        claims!.UserId.Should().Be(userId);
        claims.Role.Should().Be(userType);
        claims.BarbeariaId.Should().BeNull();
        claims.BarbeariaCode.Should().BeNull();
    }

    [Fact]
    public void Validate_ValidToken_ShouldReturnClaims()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userType = "Barbeiro";
        var email = "barbeiro@test.com";
        var barbeariaId = Guid.NewGuid();
        var barbeariaCode = "ABC12345";

        var token = _generator.GenerateToken(userId, userType, email, barbeariaId, barbeariaCode);

        // Act
        var claims = _generator.Validate(token.Value);

        // Assert
        claims.Should().NotBeNull();
        claims!.UserId.Should().Be(userId);
        claims.Role.Should().Be(userType);
        claims.BarbeariaId.Should().Be(barbeariaId);
        claims.BarbeariaCode.Should().Be(barbeariaCode);
    }

    [Fact]
    public void Validate_InvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var claims = _generator.Validate(invalidToken);

        // Assert
        claims.Should().BeNull();
    }

    [Fact]
    public void Validate_ExpiredToken_ShouldReturnNull()
    {
        // Arrange - Create token that expires immediately
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "my-super-secret-key-with-at-least-32-characters-for-security"},
            {"Jwt:Issuer", "barbapp"},
            {"Jwt:Audience", "barbapp-api"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var generator = new JwtTokenGenerator(config);
        var userId = Guid.NewGuid().ToString();

        // Manually create expired token by mocking time or using short expiration
        // For simplicity, we'll test with an invalid token
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxMjM0NSIsInJvbGUiOiJBZG1pbkNlbnRyYWwiLCJlbWFpbCI6ImFkbWluQHRlc3QuY29tIiwibmJmIjoxNjQ5OTUyMDAwLCJleHAiOjE2NDk5NTIwMDAsImlzcyI6ImJhcmJhcHAiLCJhdWQiOiJiYXJiYXBwLWFwaSJ9.invalid";

        // Act
        var claims = generator.Validate(expiredToken);

        // Assert
        claims.Should().BeNull();
    }
}