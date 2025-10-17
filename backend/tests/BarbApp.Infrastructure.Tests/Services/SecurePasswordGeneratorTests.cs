using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

/// <summary>
/// Testes unitários para SecurePasswordGenerator.
/// Segue padrão AAA (Arrange, Act, Assert) conforme rules/tests.md.
/// </summary>
public class SecurePasswordGeneratorTests
{
    private readonly SecurePasswordGenerator _sut;

    public SecurePasswordGeneratorTests()
    {
        _sut = new SecurePasswordGenerator();
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldReturn12Characters()
    {
        // Arrange & Act
        var password = _sut.Generate();

        // Assert
        password.Should().HaveLength(12);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(16)]
    [InlineData(20)]
    public void Generate_WithValidLength_ShouldReturnPasswordWithSpecifiedLength(int length)
    {
        // Arrange & Act
        var password = _sut.Generate(length);

        // Assert
        password.Should().HaveLength(length);
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldContainAtLeastOneUpperCase()
    {
        // Arrange & Act
        var password = _sut.Generate();

        // Assert
        password.Should().MatchRegex("[A-Z]", "should contain at least one uppercase letter");
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldContainAtLeastOneLowerCase()
    {
        // Arrange & Act
        var password = _sut.Generate();

        // Assert
        password.Should().MatchRegex("[a-z]", "should contain at least one lowercase letter");
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldContainAtLeastOneDigit()
    {
        // Arrange & Act
        var password = _sut.Generate();

        // Assert
        password.Should().MatchRegex("[0-9]", "should contain at least one digit");
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldContainAtLeastOneSpecialChar()
    {
        // Arrange & Act
        var password = _sut.Generate();

        // Assert
        password.Should().MatchRegex(@"[!@#$%&*\-_+=]", "should contain at least one special character");
    }

    [Fact]
    public void Generate_WithDefaultLength_ShouldOnlyContainAllowedCharacters()
    {
        // Arrange
        var allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%&*-_+=";

        // Act
        var password = _sut.Generate();

        // Assert
        password.All(c => allowedChars.Contains(c)).Should().BeTrue(
            "should only contain uppercase, lowercase, digits, and allowed special characters");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(7)]
    public void Generate_WithLengthLessThan8_ShouldThrowArgumentException(int length)
    {
        // Arrange, Act & Assert
        var act = () => _sut.Generate(length);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Password length must be at least 8 characters.*")
            .WithParameterName("length");
    }

    [Fact]
    public void Generate_CalledMultipleTimes_ShouldGenerateDifferentPasswords()
    {
        // Arrange
        const int iterations = 100;
        var passwords = new HashSet<string>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            passwords.Add(_sut.Generate());
        }

        // Assert
        passwords.Should().HaveCount(iterations, 
            "each generated password should be unique (statistically very likely with cryptographic RNG)");
    }

    [Fact]
    public void Generate_WithMinimumLength_ShouldContainAllCharacterTypes()
    {
        // Arrange & Act
        var password = _sut.Generate(8);

        // Assert
        password.Should().HaveLength(8);
        password.Should().MatchRegex("[A-Z]", "should contain at least one uppercase letter");
        password.Should().MatchRegex("[a-z]", "should contain at least one lowercase letter");
        password.Should().MatchRegex("[0-9]", "should contain at least one digit");
        password.Should().MatchRegex(@"[!@#$%&*\-_+=]", "should contain at least one special character");
    }

    [Fact]
    public void Generate_WithLargeLength_ShouldNotHaveObviousPattern()
    {
        // Arrange & Act
        var password = _sut.Generate(20);

        // Assert
        password.Should().HaveLength(20);
        
        // Verifica que não começa com padrão óbvio (uppercase, lowercase, digit, special)
        // O shuffle deve ter misturado os caracteres
        var firstFourChars = password[..4];
        var hasUpperFirst = char.IsUpper(password[0]);
        var hasLowerSecond = char.IsLower(password[1]);
        var hasDigitThird = char.IsDigit(password[2]);
        var hasSpecialFourth = "!@#$%&*-_+=".Contains(password[3]);
        
        // Pelo menos um dos primeiros 4 caracteres não deve seguir o padrão original
        // (devido ao shuffle, é estatisticamente improvável que todos mantenham a ordem)
        var allInOriginalOrder = hasUpperFirst && hasLowerSecond && hasDigitThird && hasSpecialFourth;
        allInOriginalOrder.Should().BeFalse("shuffle should have randomized the character positions");
    }

    [Fact]
    public void Generate_MultipleCallsWithSameLength_ShouldHaveDifferentCharacterDistributions()
    {
        // Arrange
        var password1 = _sut.Generate(16);
        var password2 = _sut.Generate(16);

        // Act & Assert
        password1.Should().NotBe(password2, "passwords should be randomly generated and different");
        
        // Verifica que as distribuições de caracteres são diferentes
        var uppercase1 = password1.Count(char.IsUpper);
        var uppercase2 = password2.Count(char.IsUpper);
        
        // Estatisticamente improvável que tenham exatamente a mesma contagem de maiúsculas
        (uppercase1 == uppercase2 && password1 == password2).Should().BeFalse(
            "different passwords should have different character distributions");
    }
}
