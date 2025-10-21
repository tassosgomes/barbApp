using BarbApp.Application.DTOs;
using BarbApp.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BarbApp.Application.Tests.Validators;

public class CreateLandingPageInputValidatorTests
{
    private readonly CreateLandingPageInputValidator _validator;

    public CreateLandingPageInputValidatorTests()
    {
        _validator = new CreateLandingPageInputValidator();
    }

    [Fact]
    public void Validate_ValidInput_ShouldPass()
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: "https://example.com/logo.png",
            AboutText: "About text",
            OpeningHours: "Mon-Fri: 9-18",
            InstagramUrl: "https://instagram.com/barbershop",
            FacebookUrl: "https://facebook.com/barbershop",
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidInputWithServices_ShouldPass()
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 3,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511987654321",
            Services: new List<ServiceDisplayInput>
            {
                new(Guid.NewGuid(), 0, true),
                new(Guid.NewGuid(), 1, false)
            }
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyBarbershopId_ShouldFail()
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.Empty,
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BarbershopId)
            .WithErrorMessage("ID da barbearia é obrigatório");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public void Validate_InvalidTemplateId_ShouldFail(int invalidTemplateId)
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: invalidTemplateId,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
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
    [InlineData(null)]
    public void Validate_EmptyWhatsappNumber_ShouldFail(string whatsapp)
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: whatsapp,
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WhatsappNumber);
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
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
    public void Validate_LogoUrlTooLong_ShouldFail()
    {
        // Arrange
        var longUrl = "https://example.com/" + new string('a', 500);
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: longUrl,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: invalidUrl,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LogoUrl)
            .WithErrorMessage("Logo URL deve ser uma URL válida");
    }

    [Fact]
    public void Validate_AboutTextTooLong_ShouldFail()
    {
        // Arrange
        var longText = new string('a', 2001);
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: longText,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AboutText)
            .WithErrorMessage("Texto 'Sobre' deve ter no máximo 2000 caracteres");
    }

    [Fact]
    public void Validate_OpeningHoursTooLong_ShouldFail()
    {
        // Arrange
        var longText = new string('a', 501);
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: longText,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OpeningHours)
            .WithErrorMessage("Horário deve ter no máximo 500 caracteres");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://instagram.com")]
    public void Validate_InvalidInstagramUrl_ShouldFail(string invalidUrl)
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: invalidUrl,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: longUrl,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
            Services: null
        );

        // Act
        var result = _validator.TestValidate(input);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InstagramUrl)
            .WithErrorMessage("Instagram URL deve ter no máximo 255 caracteres");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://facebook.com")]
    public void Validate_InvalidFacebookUrl_ShouldFail(string invalidUrl)
    {
        // Arrange
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: invalidUrl,
            WhatsappNumber: "+5511999999999",
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: longUrl,
            WhatsappNumber: "+5511999999999",
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
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
        var input = new CreateLandingPageInput(
            BarbershopId: Guid.NewGuid(),
            TemplateId: 1,
            LogoUrl: null,
            AboutText: null,
            OpeningHours: null,
            InstagramUrl: null,
            FacebookUrl: null,
            WhatsappNumber: "+5511999999999",
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
}
