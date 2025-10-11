// BarbApp.Infrastructure.Tests/Services/PasswordHasherTests.cs
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = _hasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password); // Hash should be different from plain password
        hash.Should().StartWith("$2"); // BCrypt hashes start with $2
    }

    [Fact]
    public void Hash_SamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash1 = _hasher.Hash(password);
        var hash2 = _hasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt generates different hashes for same password
        hash1.Should().StartWith("$2");
        hash2.Should().StartWith("$2");
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyPassword = "";
        var hash = _hasher.Hash("somepassword");

        // Act & Assert
        Action act = () => _hasher.Verify(emptyPassword, hash);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or whitespace*");
    }

    [Fact]
    public void Verify_WhitespacePassword_ShouldThrowArgumentException()
    {
        // Arrange
        var whitespacePassword = "   ";
        var hash = _hasher.Hash("somepassword");

        // Act & Assert
        Action act = () => _hasher.Verify(whitespacePassword, hash);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or whitespace*");
    }

    [Fact]
    public void Verify_EmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var emptyHash = "";

        // Act & Assert
        Action act = () => _hasher.Verify(password, emptyHash);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password hash cannot be null or whitespace*");
    }

    [Fact]
    public void Hash_EmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyPassword = "";

        // Act & Assert
        Action act = () => _hasher.Hash(emptyPassword);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or whitespace*");
    }

    [Fact]
    public void Hash_WhitespacePassword_ShouldThrowArgumentException()
    {
        // Arrange
        var whitespacePassword = "   ";

        // Act & Assert
        Action act = () => _hasher.Hash(whitespacePassword);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or whitespace*");
    }
}