// BarbApp.Application.Tests/Validators/LoginAdminBarbeariaInputValidatorTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BarbApp.Application.Tests.Validators;

public class LoginAdminBarbeariaInputValidatorTests
{
    private readonly LoginAdminBarbeariaInputValidator _validator;

    public LoginAdminBarbeariaInputValidatorTests()
    {
        _validator = new LoginAdminBarbeariaInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = "admin@barbearia.com",
            Senha = "password123",
            Codigo = "ABC12345"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = email,
            Senha = "password123",
            Codigo = "ABC12345"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email é obrigatório");
    }

    [Theory]
    [InlineData("invalid-email")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = email,
            Senha = "password123",
            Codigo = "ABC12345"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string senha)
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = "admin@barbearia.com",
            Senha = senha,
            Codigo = "ABC12345"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Senha)
            .WithErrorMessage("Senha é obrigatória");
    }

    [Theory]
    [InlineData("12345")]
    public void Validate_PasswordTooShort_ShouldHaveValidationError(string senha)
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = "admin@barbearia.com",
            Senha = senha,
            Codigo = "ABC12345"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Senha)
            .WithErrorMessage("Senha deve ter no mínimo 6 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCodigo_ShouldHaveValidationError(string codigo)
    {
        // Arrange
        var input = new LoginAdminBarbeariaInput
        {
            Email = "admin@barbearia.com",
            Senha = "password123",
            Codigo = codigo
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Codigo)
            .WithErrorMessage("Código da barbearia é obrigatório");
    }
}