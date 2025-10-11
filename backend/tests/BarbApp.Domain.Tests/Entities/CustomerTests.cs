// BarbApp.Domain.Tests/Entities/CustomerTests.cs
using BarbApp.Domain.Entities;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class CustomerTests
{
    [Theory]
    [InlineData("11987654321")]
    [InlineData("1134567890")]
    [InlineData("(11) 98765-4321")] // Deve limpar formatação
    [InlineData("+55 11 98765-4321")] // Deve limpar +55 e formatação
    public void Create_ValidPhone_ShouldSucceed(string telefone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var customer = Customer.Create(barbeariaId, telefone, "João Silva");

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBe(Guid.Empty);
        customer.BarbeariaId.Should().Be(barbeariaId);
        customer.Telefone.Should().MatchRegex(@"^\d{10,11}$");
        customer.Name.Should().Be("João Silva");
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("123")] // Muito curto
    [InlineData("123456789012")] // Muito longo
    [InlineData("")] // Vazio
    [InlineData(" ")] // Espaço
    [InlineData(null)] // Null
    public void Create_InvalidPhone_ShouldThrowException(string telefone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Customer.Create(barbeariaId, telefone, "João Silva");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("abc123")] // Não numérico
    [InlineData("11987")] // Menos de 10 dígitos
    [InlineData("119876543210")] // Mais de 11 dígitos
    public void Create_InvalidPhoneFormat_ShouldThrowException(string telefone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Customer.Create(barbeariaId, telefone, "João Silva");
        act.Should().Throw<ArgumentException>().WithMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
    }

    [Fact]
    public void Create_EmptyBarbeariaId_ShouldThrowException()
    {
        // Arrange
        var barbeariaId = Guid.Empty;
        var telefone = "11987654321";

        // Act & Assert
        Action act = () => Customer.Create(barbeariaId, telefone, "João Silva");
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
        var telefone = "11987654321";

        // Act & Assert
        Action act = () => Customer.Create(barbeariaId, telefone, name);
        act.Should().Throw<ArgumentException>().WithMessage("Name is required");
    }
}