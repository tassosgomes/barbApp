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
            Email = "test@test.com",
            Password = "password123"
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
        var input = new LoginBarbeiroInput
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("E-mail é obrigatório");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("E-mail inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = "test@test.com",
            Password = password
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Senha é obrigatória");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc")]
    public void Validate_ShortPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var input = new LoginBarbeiroInput
        {
            Email = "test@test.com",
            Password = password
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("A senha deve ter no mínimo 6 caracteres");
    }
}
