// BarbApp.Domain.Tests/Entities/AdminCentralUserTests.cs
using BarbApp.Domain.Entities;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class AdminCentralUserTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        // Arrange
        var email = "admin@barbapp.com";
        var passwordHash = "hashedpassword";
        var name = "Admin Central";

        // Act
        var user = AdminCentralUser.Create(email, passwordHash, name);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.PasswordHash.Should().Be(passwordHash);
        user.Name.Should().Be(name);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "hash", "name")]
    [InlineData(" ", "hash", "name")]
    [InlineData(null, "hash", "name")]
    public void Create_InvalidEmail_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Act & Assert
        Action act = () => AdminCentralUser.Create(email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Email is required");
    }

    [Theory]
    [InlineData("email@test.com", "", "name")]
    [InlineData("email@test.com", " ", "name")]
    [InlineData("email@test.com", null, "name")]
    public void Create_InvalidPasswordHash_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Act & Assert
        Action act = () => AdminCentralUser.Create(email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Password hash is required");
    }

    [Theory]
    [InlineData("email@test.com", "hash", "")]
    [InlineData("email@test.com", "hash", " ")]
    [InlineData("email@test.com", "hash", null)]
    public void Create_InvalidName_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Act & Assert
        Action act = () => AdminCentralUser.Create(email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Name is required");
    }

    [Fact]
    public void VerifyPassword_CorrectHash_ShouldReturnTrue()
    {
        // Arrange
        var user = AdminCentralUser.Create("admin@test.com", "correcthash", "Admin");

        // Act
        var result = user.VerifyPassword("correcthash");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectHash_ShouldReturnFalse()
    {
        // Arrange
        var user = AdminCentralUser.Create("admin@test.com", "correcthash", "Admin");

        // Act
        var result = user.VerifyPassword("wronghash");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = AdminCentralUser.Create("admin@test.com", "hash", "Admin");

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}