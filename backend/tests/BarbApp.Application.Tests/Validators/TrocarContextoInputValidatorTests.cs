// BarbApp.Application.Tests/Validators/TrocarContextoInputValidatorTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BarbApp.Application.Tests.Validators;

public class TrocarContextoInputValidatorTests
{
    private readonly TrocarContextoInputValidator _validator;

    public TrocarContextoInputValidatorTests()
    {
        _validator = new TrocarContextoInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyNovaBarbeariaId_ShouldHaveValidationError()
    {
        // Arrange
        var input = new TrocarContextoInput
        {
            NovaBarbeariaId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NovaBarbeariaId)
            .WithErrorMessage("NovaBarbeariaId é obrigatório");
    }
}