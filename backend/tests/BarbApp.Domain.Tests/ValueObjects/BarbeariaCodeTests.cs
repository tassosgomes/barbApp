// BarbApp.Domain.Tests/ValueObjects/BarbeariaCodeTests.cs
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;

namespace BarbApp.Domain.Tests.ValueObjects;

public class BarbeariaCodeTests
{
    [Theory]
    [InlineData("ABC23456")]
    [InlineData("XYZ98765")]
    [InlineData("22334455")]
    [InlineData("abcd2345")] // Deve converter para uppercase
    public void Create_ValidCode_ShouldSucceed(string codeValue)
    {
        // Act
        var code = BarbeariaCode.Create(codeValue);

        // Assert
        code.Value.Should().Be(codeValue.ToUpperInvariant());
    }

    [Theory]
    [InlineData("ABC")] // Muito curto
    [InlineData("ABCDEFGHIJ")] // Muito longo
    [InlineData("ABC1234O")] // Contém O
    [InlineData("ABC1234I")] // Contém I
    [InlineData("ABC12340")] // Contém 0
    [InlineData("ABC12341")] // Contém 1
    [InlineData("")] // Vazio
    [InlineData(null)] // Null
    public void Create_InvalidCode_ShouldThrowException(string codeValue)
    {
        // Act & Assert
        Action act = () => BarbeariaCode.Create(codeValue);
        act.Should().Throw<InvalidBarbeariaCodeException>();
    }

    [Fact]
    public void Equals_SameCodes_ShouldBeEqual()
    {
        // Arrange
        var code1 = BarbeariaCode.Create("ABC23456");
        var code2 = BarbeariaCode.Create("ABC23456");

        // Act & Assert
        code1.Should().Be(code2);
        (code1 == code2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCodes_ShouldNotBeEqual()
    {
        // Arrange
        var code1 = BarbeariaCode.Create("ABC23456");
        var code2 = BarbeariaCode.Create("XYZ98765");

        // Act & Assert
        code1.Should().NotBe(code2);
        (code1 == code2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var code = BarbeariaCode.Create("ABC23456");

        // Act
        var result = code.ToString();

        // Assert
        result.Should().Be("ABC23456");
    }

    [Fact]
    public void GetHashCode_SameCodes_ShouldBeEqual()
    {
        // Arrange
        var code1 = BarbeariaCode.Create("ABC23456");
        var code2 = BarbeariaCode.Create("ABC23456");

        // Act & Assert
        code1.GetHashCode().Should().Be(code2.GetHashCode());
    }
}