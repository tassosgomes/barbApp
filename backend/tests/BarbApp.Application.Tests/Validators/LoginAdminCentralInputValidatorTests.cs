// BarbApp.Application.Tests/Validators/LoginAdminCentralInputValidatorTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BarbApp.Application.Tests.Validators;

public class LoginAdminCentralInputValidatorTests
{
    private readonly LoginAdminCentralInputValidator _validator;

    public LoginAdminCentralInputValidatorTests()
    {
        _validator = new LoginAdminCentralInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = "admin@barbapp.com",
            Senha = "password123"
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
        var input = new LoginAdminCentralInput
        {
            Email = email,
            Senha = "password123"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email é obrigatório");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("admin@")]
    [InlineData("@barbapp.com")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = email,
            Senha = "password123"
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
        var input = new LoginAdminCentralInput
        {
            Email = "admin@barbapp.com",
            Senha = senha
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Senha)
            .WithErrorMessage("Senha é obrigatória");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("123")]
    public void Validate_PasswordTooShort_ShouldHaveValidationError(string senha)
    {
        // Arrange
        var input = new LoginAdminCentralInput
        {
            Email = "admin@barbapp.com",
            Senha = senha
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Senha)
            .WithErrorMessage("Senha deve ter no mínimo 6 caracteres");
    }
}