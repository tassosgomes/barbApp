using BarbApp.Domain.Entities;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class BarbershopServiceTests
{
    private const string ValidName = "Corte de Cabelo";
    private const string ValidDescription = "Corte simples masculino";
    private const int ValidDuration = 30;
    private const decimal ValidPrice = 50.00m;

    [Fact]
    public void Create_ValidParameters_ShouldSucceed()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var service = BarbershopService.Create(
            barbeariaId,
            ValidName,
            ValidDescription,
            ValidDuration,
            ValidPrice);

        // Assert
        service.Should().NotBeNull();
        service.Id.Should().NotBe(Guid.Empty);
        service.BarbeariaId.Should().Be(barbeariaId);
        service.Name.Should().Be(ValidName);
        service.Description.Should().Be(ValidDescription);
        service.DurationMinutes.Should().Be(ValidDuration);
        service.Price.Should().Be(ValidPrice);
        service.IsActive.Should().BeTrue();
        service.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        service.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithoutDescription_ShouldSucceed()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var service = BarbershopService.Create(
            barbeariaId,
            ValidName,
            null,
            ValidDuration,
            ValidPrice);

        // Assert
        service.Description.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyBarbeariaId_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.Empty;

        // Act & Assert
        Action act = () => BarbershopService.Create(
            barbeariaId,
            ValidName,
            ValidDescription,
            ValidDuration,
            ValidPrice);
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
        Action act = () => BarbershopService.Create(
            barbeariaId,
            name,
            ValidDescription,
            ValidDuration,
            ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("Service name is required");
    }

    [Fact]
    public void Create_NameTooLong_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var longName = new string('a', 101);

        // Act & Assert
        Action act = () => BarbershopService.Create(
            barbeariaId,
            longName,
            ValidDescription,
            ValidDuration,
            ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("*100 characters*");
    }

    [Fact]
    public void Create_DescriptionTooLong_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var longDescription = new string('a', 501);

        // Act & Assert
        Action act = () => BarbershopService.Create(
            barbeariaId,
            ValidName,
            longDescription,
            ValidDuration,
            ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("*500 characters*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Create_InvalidDuration_ShouldThrowException(int duration)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => BarbershopService.Create(
            barbeariaId,
            ValidName,
            ValidDescription,
            duration,
            ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("*duration*greater than zero*");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Create_NegativePrice_ShouldThrowException(decimal price)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => BarbershopService.Create(
            barbeariaId,
            ValidName,
            ValidDescription,
            ValidDuration,
            price);
        act.Should().Throw<ArgumentException>().WithMessage("*price*zero or greater*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(100)]
    public void Create_ValidPrice_ShouldSucceed(decimal price)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var service = BarbershopService.Create(
            barbeariaId,
            ValidName,
            ValidDescription,
            ValidDuration,
            price);

        // Assert
        service.Price.Should().Be(price);
    }

    [Fact]
    public void Update_ValidParameters_ShouldUpdateFields()
    {
        // Arrange
        var service = CreateValidService();
        var newName = "Corte + Barba";
        var newDescription = "Corte de cabelo com barba";
        var newDuration = 60;
        var newPrice = 80.00m;

        // Act
        service.Update(newName, newDescription, newDuration, newPrice);

        // Assert
        service.Name.Should().Be(newName);
        service.Description.Should().Be(newDescription);
        service.DurationMinutes.Should().Be(newDuration);
        service.Price.Should().Be(newPrice);
        service.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_InvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Action act = () => service.Update(name, ValidDescription, ValidDuration, ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("Service name is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_InvalidDuration_ShouldThrowException(int duration)
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Action act = () => service.Update(ValidName, ValidDescription, duration, ValidPrice);
        act.Should().Throw<ArgumentException>().WithMessage("*duration*greater than zero*");
    }

    [Fact]
    public void Update_NegativePrice_ShouldThrowException()
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Action act = () => service.Update(ValidName, ValidDescription, ValidDuration, -10m);
        act.Should().Throw<ArgumentException>().WithMessage("*price*zero or greater*");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var service = CreateValidService();
        service.Deactivate();

        // Act
        service.Activate();

        // Assert
        service.IsActive.Should().BeTrue();
        service.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var service = CreateValidService();

        // Act
        service.Deactivate();

        // Assert
        service.IsActive.Should().BeFalse();
        service.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var nameWithSpaces = "  Corte de Cabelo  ";
        var descriptionWithSpaces = "  Corte simples masculino  ";

        // Act
        var service = BarbershopService.Create(
            barbeariaId,
            nameWithSpaces,
            descriptionWithSpaces,
            ValidDuration,
            ValidPrice);

        // Assert
        service.Name.Should().Be("Corte de Cabelo");
        service.Description.Should().Be("Corte simples masculino");
    }

    [Fact]
    public void Update_ShouldTrimNameAndDescription()
    {
        // Arrange
        var service = CreateValidService();
        var nameWithSpaces = "  Corte + Barba  ";
        var descriptionWithSpaces = "  Corte com barba  ";

        // Act
        service.Update(nameWithSpaces, descriptionWithSpaces, ValidDuration, ValidPrice);

        // Assert
        service.Name.Should().Be("Corte + Barba");
        service.Description.Should().Be("Corte com barba");
    }

    private static BarbershopService CreateValidService()
    {
        return BarbershopService.Create(
            Guid.NewGuid(),
            ValidName,
            ValidDescription,
            ValidDuration,
            ValidPrice);
    }
}
