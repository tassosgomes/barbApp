using BarbApp.Domain.Entities;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class BarberTests
{
    private const string ValidEmail = "barber@example.com";
    private const string ValidPasswordHash = "hashedpassword123";
    private const string ValidPhone = "11987654321";
    private const string ValidName = "João Silva";

    [Fact]
    public void Create_ValidParameters_ShouldSucceed()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var serviceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var barber = Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone,
            serviceIds);

        // Assert
        barber.Should().NotBeNull();
        barber.Id.Should().NotBe(Guid.Empty);
        barber.BarbeariaId.Should().Be(barbeariaId);
        barber.Name.Should().Be(ValidName);
        barber.Email.Should().Be(ValidEmail.ToLowerInvariant());
        barber.PasswordHash.Should().Be(ValidPasswordHash);
        barber.Phone.Should().MatchRegex(@"^\d{10,11}$");
        barber.ServiceIds.Should().BeEquivalentTo(serviceIds);
        barber.IsActive.Should().BeTrue();
        barber.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithoutServiceIds_ShouldCreateEmptyList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var barber = Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone);

        // Assert
        barber.ServiceIds.Should().NotBeNull();
        barber.ServiceIds.Should().BeEmpty();
    }

    [Theory]
    [InlineData("11987654321")]
    [InlineData("1134567890")]
    [InlineData("(11) 98765-4321")]
    [InlineData("+55 11 98765-4321")]
    public void Create_ValidPhone_ShouldCleanAndNormalize(string phone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var barber = Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            phone);

        // Assert
        barber.Phone.Should().MatchRegex(@"^\d{10,11}$");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_InvalidPhone_ShouldThrowException(string phone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            phone);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("barber@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public void Create_ValidEmail_ShouldSucceed(string email)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var barber = Barber.Create(
            barbeariaId,
            ValidName,
            email,
            ValidPasswordHash,
            ValidPhone);

        // Assert
        barber.Email.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    public void Create_InvalidEmail_ShouldThrowException(string email)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            ValidName,
            email,
            ValidPasswordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_InvalidPasswordHash_ShouldThrowException(string passwordHash)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            passwordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*password*");
    }

    [Fact]
    public void Create_EmptyBarbeariaId_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.Empty;

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("Barbearia ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_InvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            name,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("Name is required");
    }

    [Fact]
    public void Create_NameTooLong_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var longName = new string('a', 101);

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            longName,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*100 characters*");
    }

    [Fact]
    public void Create_EmailTooLong_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var longEmail = new string('a', 250) + "@test.com";

        // Act & Assert
        Action act = () => Barber.Create(
            barbeariaId,
            ValidName,
            longEmail,
            ValidPasswordHash,
            ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("*255 characters*");
    }

    [Fact]
    public void Update_ValidParameters_ShouldUpdateFields()
    {
        // Arrange
        var barber = CreateValidBarber();
        var newName = "Maria Santos";
        var newPhone = "21987654321";
        var newServiceIds = new List<Guid> { Guid.NewGuid() };

        // Act
        barber.Update(newName, newPhone, newServiceIds);

        // Assert
        barber.Name.Should().Be(newName);
        barber.Phone.Should().MatchRegex(@"^\d{10,11}$");
        barber.ServiceIds.Should().BeEquivalentTo(newServiceIds);
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_InvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var barber = CreateValidBarber();

        // Act & Assert
        Action act = () => barber.Update(name, ValidPhone);
        act.Should().Throw<ArgumentException>().WithMessage("Name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("123")]
    public void Update_InvalidPhone_ShouldThrowException(string phone)
    {
        // Arrange
        var barber = CreateValidBarber();

        // Act & Assert
        Action act = () => barber.Update(ValidName, phone);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateEmail_ValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var barber = CreateValidBarber();
        var newEmail = "newemail@example.com";

        // Act
        barber.UpdateEmail(newEmail);

        // Assert
        barber.Email.Should().Be(newEmail.ToLowerInvariant());
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("notanemail")]
    public void UpdateEmail_InvalidEmail_ShouldThrowException(string email)
    {
        // Arrange
        var barber = CreateValidBarber();

        // Act & Assert
        Action act = () => barber.UpdateEmail(email);
        act.Should().Throw<ArgumentException>().WithMessage("*email*");
    }

    [Fact]
    public void ChangePassword_ValidPasswordHash_ShouldUpdatePassword()
    {
        // Arrange
        var barber = CreateValidBarber();
        var newPasswordHash = "newhashedpassword456";

        // Act
        barber.ChangePassword(newPasswordHash);

        // Assert
        barber.PasswordHash.Should().Be(newPasswordHash);
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangePassword_InvalidPasswordHash_ShouldThrowException(string passwordHash)
    {
        // Arrange
        var barber = CreateValidBarber();

        // Act & Assert
        Action act = () => barber.ChangePassword(passwordHash);
        act.Should().Throw<ArgumentException>().WithMessage("*password*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var barber = CreateValidBarber();

        // Act
        barber.Deactivate();

        // Assert
        barber.IsActive.Should().BeFalse();
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var barber = CreateValidBarber();
        barber.Deactivate();

        // Act
        barber.Activate();

        // Assert
        barber.IsActive.Should().BeTrue();
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static Barber CreateValidBarber()
    {
        return Barber.Create(
            Guid.NewGuid(),
            ValidName,
            ValidEmail,
            ValidPasswordHash,
            ValidPhone);
    }
}