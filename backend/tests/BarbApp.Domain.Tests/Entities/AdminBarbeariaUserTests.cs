// BarbApp.Domain.Tests/Entities/AdminBarbeariaUserTests.cs
using BarbApp.Domain.Entities;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class AdminBarbeariaUserTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var email = "admin@barbearia.com";
        var passwordHash = "hashedpassword";
        var name = "Admin Barbearia";

        // Act
        var user = AdminBarbeariaUser.Create(barbeariaId, email, passwordHash, name);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.BarbeariaId.Should().Be(barbeariaId);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.PasswordHash.Should().Be(passwordHash);
        user.Name.Should().Be(name);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_EmptyBarbeariaId_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.Empty;
        var email = "admin@barbearia.com";
        var passwordHash = "hashedpassword";
        var name = "Admin Barbearia";

        // Act & Assert
        Action act = () => AdminBarbeariaUser.Create(barbeariaId, email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Barbearia ID is required");
    }

    [Theory]
    [InlineData("", "hash", "name")]
    [InlineData(" ", "hash", "name")]
    [InlineData(null, "hash", "name")]
    public void Create_InvalidEmail_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => AdminBarbeariaUser.Create(barbeariaId, email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Email is required");
    }

    [Theory]
    [InlineData("email@test.com", "", "name")]
    [InlineData("email@test.com", " ", "name")]
    [InlineData("email@test.com", null, "name")]
    public void Create_InvalidPasswordHash_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => AdminBarbeariaUser.Create(barbeariaId, email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Password hash is required");
    }

    [Theory]
    [InlineData("email@test.com", "hash", "")]
    [InlineData("email@test.com", "hash", " ")]
    [InlineData("email@test.com", "hash", null)]
    public void Create_InvalidName_ShouldThrowException(string email, string passwordHash, string name)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => AdminBarbeariaUser.Create(barbeariaId, email, passwordHash, name);
        act.Should().Throw<ArgumentException>().WithMessage("Name is required");
    }

    [Fact]
    public void VerifyPassword_CorrectHash_ShouldReturnTrue()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var user = AdminBarbeariaUser.Create(barbeariaId, "admin@test.com", "correcthash", "Admin");

        // Act
        var result = user.VerifyPassword("correcthash");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectHash_ShouldReturnFalse()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var user = AdminBarbeariaUser.Create(barbeariaId, "admin@test.com", "correcthash", "Admin");

        // Act
        var result = user.VerifyPassword("wronghash");

        // Assert
        result.Should().BeFalse();
    }
}