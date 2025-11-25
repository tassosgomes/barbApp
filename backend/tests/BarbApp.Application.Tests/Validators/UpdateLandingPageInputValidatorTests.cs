using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BarbApp.Application.Tests.Validators;

public class UpdateLandingPageInputValidatorTests
{
    private readonly UpdateLandingPageInputValidator _validator;

    public UpdateLandingPageInputValidatorTests()
    {
        _validator = new UpdateLandingPageInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldPass()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: 2,
            LogoUrl: "https://example.com/new-logo.png",
            AboutText: "Updated about text",
            OpeningHours: "Mon-Sat: 8-20",
            InstagramUrl: "https://instagram.com/newhandle",
            FacebookUrl: "https://facebook.com/newpage",
            WhatsappNumber: "+5521987654321",
            Services: new List<ServiceDisplayInput>
            {
                new(Guid.NewGuid(), 0, true)
            }
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllNullFields_ShouldPass()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_OnlyTemplateId_ShouldPass()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: 4,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Validate_InvalidTemplateId_ShouldFail(int invalidTemplateId)
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: invalidTemplateId,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage("Template deve estar entre 1 e 5");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyWhatsappNumber_ShouldPass(string emptyWhatsapp)
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: emptyWhatsapp,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.WhatsappNumber);
    }

    [Theory]
    [InlineData("11999999999")]
    [InlineData("+55119999")]
    [InlineData("+551199999999999")]
    [InlineData("5511999999999")]
    [InlineData("+55 11 99999-9999")]
    [InlineData("invalid")]
    public void Validate_InvalidWhatsappNumberFormat_ShouldFail(string invalidWhatsapp)
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: invalidWhatsapp,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WhatsappNumber)
            .WithErrorMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX");
    }

    [Fact]
    public void Validate_EmptyLogoUrl_ShouldPass()
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: "",
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.LogoUrl);
    }

    [Fact]
    public void Validate_LogoUrlTooLong_ShouldFail()
    {
        // Arrange
        var longUrl = "https://example.com/" + new string('a', 500);
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: longUrl,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LogoUrl)
            .WithErrorMessage("Logo URL deve ter no máximo 500 caracteres");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert('xss')")]
    public void Validate_InvalidLogoUrl_ShouldFail(string invalidUrl)
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: invalidUrl,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LogoUrl)
            .WithErrorMessage("Logo URL deve ser uma URL válida");
    }

    [Fact]
    public void Validate_EmptyAboutText_ShouldPass()
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: "",
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.AboutText);
    }

    [Fact]
    public void Validate_AboutTextTooLong_ShouldFail()
    {
        // Arrange
        var longText = new string('a', 2001);
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: longText,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AboutText)
            .WithErrorMessage("Texto 'Sobre' deve ter no máximo 2000 caracteres");
    }

    [Fact]
    public void Validate_EmptyOpeningHours_ShouldPass()
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: "",
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.OpeningHours);
    }

    [Fact]
    public void Validate_OpeningHoursTooLong_ShouldFail()
    {
        // Arrange
        var longText = new string('a', 501);
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: longText,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpeningHours)
            .WithErrorMessage("Horário deve ter no máximo 500 caracteres");
    }

    [Fact]
    public void Validate_EmptyInstagramUrl_ShouldPass()
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: "",
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.InstagramUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://instagram.com")]
    public void Validate_InvalidInstagramUrl_ShouldFail(string invalidUrl)
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: invalidUrl,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InstagramUrl)
            .WithErrorMessage("Instagram URL deve ser uma URL válida");
    }

    [Fact]
    public void Validate_InstagramUrlTooLong_ShouldFail()
    {
        // Arrange
        var longUrl = "https://instagram.com/" + new string('a', 250);
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: longUrl,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InstagramUrl)
            .WithErrorMessage("Instagram URL deve ter no máximo 255 caracteres");
    }

    [Fact]
    public void Validate_EmptyFacebookUrl_ShouldPass()
    {
        // Arrange - Empty strings are allowed (to clear a field)
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: "",
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert - Should NOT have error because empty strings are allowed
        result.ShouldNotHaveValidationErrorFor(x => x.FacebookUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://facebook.com")]
    public void Validate_InvalidFacebookUrl_ShouldFail(string invalidUrl)
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: invalidUrl,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FacebookUrl)
            .WithErrorMessage("Facebook URL deve ser uma URL válida");
    }

    [Fact]
    public void Validate_FacebookUrlTooLong_ShouldFail()
    {
        // Arrange
        var longUrl = "https://facebook.com/" + new string('a', 250);
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: longUrl,
            WhatsappNumber: null,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FacebookUrl)
            .WithErrorMessage("Facebook URL deve ter no máximo 255 caracteres");
    }

    [Fact]
    public void Validate_ServiceWithEmptyServiceId_ShouldFail()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: new List<ServiceDisplayInput>
            {
                new(Guid.Empty, 0, true)
            }
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor("Services[0].ServiceId")
            .WithErrorMessage("Service ID é obrigatório");
    }

    [Fact]
    public void Validate_ServiceWithNegativeDisplayOrder_ShouldFail()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: new List<ServiceDisplayInput>
            {
                new(Guid.NewGuid(), -1, true)
            }
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor("Services[0].DisplayOrder")
            .WithErrorMessage("Display order deve ser maior ou igual a 0");
    }

    [Fact]
    public void Validate_EmptyServicesList_ShouldPass()
    {
        // Arrange
        var input = new UpdateLandingPageInput(
            TemplateId: null,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: null,
            Services: new List<ServiceDisplayInput>()
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
