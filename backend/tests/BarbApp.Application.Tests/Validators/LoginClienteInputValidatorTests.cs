// BarbApp.Application.Tests/Validators/LoginClienteInputValidatorTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BarbApp.Application.Tests.Validators;

public class LoginClienteInputValidatorTests
{
    private readonly LoginClienteInputValidator _validator;

    public LoginClienteInputValidatorTests()
    {
        _validator = new LoginClienteInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = "11987654321",
            Nome = "João Silva"
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
        var input = new LoginClienteInput
        {
            Codigo = codigo,
            Telefone = "11987654321",
            Nome = "João Silva"
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
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = telefone,
            Nome = "João Silva"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Telefone)
            .WithErrorMessage("Telefone é obrigatório");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("abcdefghij")]
    public void Validate_InvalidTelefone_ShouldHaveValidationError(string telefone)
    {
        // Arrange
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = telefone,
            Nome = "João Silva"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Telefone)
            .WithErrorMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyNome_ShouldHaveValidationError(string nome)
    {
        // Arrange
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = "11987654321",
            Nome = nome
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("Nome é obrigatório");
    }

    [Theory]
    [InlineData("A")]
    public void Validate_NomeTooShort_ShouldHaveValidationError(string nome)
    {
        // Arrange
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = "11987654321",
            Nome = nome
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("Nome deve ter no mínimo 2 caracteres");
    }

    [Fact]
    public void Validate_NomeTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var nome = new string('A', 101);
        var input = new LoginClienteInput
        {
            Codigo = "ABC12345",
            Telefone = "11987654321",
            Nome = nome
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage("Nome deve ter no máximo 100 caracteres");
    }
}