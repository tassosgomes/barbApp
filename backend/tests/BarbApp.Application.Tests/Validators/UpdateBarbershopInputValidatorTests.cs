using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BarbApp.Application.Tests.Validators;

public class UpdateBarbershopInputValidatorTests
{
    private readonly UpdateBarbershopInputValidator _validator;

    public UpdateBarbershopInputValidatorTests()
    {
        _validator = new UpdateBarbershopInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldPass()
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            "Sala 15",
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.Empty,
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("ID da barbearia é obrigatório");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_NameEmpty_ShouldFail(string name)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            name,
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome da barbearia é obrigatório");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 256);
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            longName,
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome deve ter no máximo 255 caracteres");
    }

    [Theory]
    [InlineData("1198765432")] // Too short
    [InlineData("119876543210")] // Too long
    [InlineData("(11)98765-432")] // Invalid format
    public void Validate_InvalidPhone_ShouldFail(string phone)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            phone,
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Telefone deve estar no formato (XX) XXXXX-XXXX");
    }

    [Theory]
    [InlineData("(11) 98765-4321")]
    [InlineData("(21) 99876-5432")]
    public void Validate_ValidPhone_ShouldPass(string phone)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            phone,
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("email@")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            email,
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email deve ser válido");
    }

    [Theory]
    [InlineData("0131010")] // Too short
    [InlineData("013101000")] // Too long
    [InlineData("01310-10")] // Invalid format
    public void Validate_InvalidZipCode_ShouldFail(string zipCode)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            zipCode,
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ZipCode)
            .WithErrorMessage("CEP deve estar no formato XXXXX-XXX");
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("INVALID")]
    public void Validate_InvalidState_ShouldFail(string state)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            state);

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State)
            .WithErrorMessage("Estado deve ser uma sigla válida");
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("RJ")]
    [InlineData("MG")]
    public void Validate_ValidState_ShouldPass(string state)
    {
        // Arrange
        var input = new UpdateBarbershopInput(
            Guid.NewGuid(),
            "Barbearia Teste",
            "(11) 98765-4321",
            "João Silva",
            "joao@test.com",
            "01310-100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            state);

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.State);
    }
}