// BarbApp.Application.Tests/Validators/LoginBarbeiroInputValidatorTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BarbApp.Application.Tests.Validators;

public class LoginBarbeiroInputValidatorTests
{
    private readonly LoginBarbeiroInputValidator _validator;

    public LoginBarbeiroInputValidatorTests()
    {
        _validator = new LoginBarbeiroInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Codigo = "ABC12345",
            Telefone = "11987654321"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCodigo_ShouldHaveValidationError(string codigo)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Codigo = codigo,
            Telefone = "11987654321"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Codigo)
            .WithErrorMessage("Código da barbearia é obrigatório");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyTelefone_ShouldHaveValidationError(string telefone)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Codigo = "ABC12345",
            Telefone = telefone
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Telefone)
            .WithErrorMessage("Telefone é obrigatório");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    [InlineData("abcdefghij")]
    [InlineData("11a98765432")]
    public void Validate_InvalidTelefone_ShouldHaveValidationError(string telefone)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Codigo = "ABC12345",
            Telefone = telefone
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Telefone)
            .WithErrorMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
    }
}