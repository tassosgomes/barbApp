using BarbApp.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BarbApp.Domain.Tests.Entities;

public class LandingPageConfigTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateLandingPageConfig()
    {
        var barbershopId = Guid.NewGuid();
        var templateId = 1;
        var whatsappNumber = "+5511999999999";

        var landingPageConfig = LandingPageConfig.Create(
            barbershopId,
            templateId,
            whatsappNumber);

        landingPageConfig.Should().NotBeNull();
        landingPageConfig.Id.Should().NotBeEmpty();
        landingPageConfig.BarbershopId.Should().Be(barbershopId);
        landingPageConfig.TemplateId.Should().Be(templateId);
        landingPageConfig.WhatsappNumber.Should().Be(whatsappNumber);
        landingPageConfig.IsPublished.Should().BeTrue();
        landingPageConfig.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_WithInvalidTemplateId_ShouldThrowException(int invalidTemplateId)
    {
        var barbershopId = Guid.NewGuid();
        var whatsappNumber = "+5511999999999";

        var act = () => LandingPageConfig.Create(
            barbershopId,
            invalidTemplateId,
            whatsappNumber);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Template ID must be between 1 and 5*");
    }

    [Fact]
    public void Create_WithEmptyBarbershopId_ShouldThrowException()
    {
        var act = () => LandingPageConfig.Create(
            Guid.Empty,
            1,
            "+5511999999999");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Barbershop ID is required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidWhatsappNumber_ShouldThrowException(string invalidWhatsapp)
    {
        var act = () => LandingPageConfig.Create(
            Guid.NewGuid(),
            1,
            invalidWhatsapp);

        act.Should().Throw<ArgumentException>()
            .WithMessage("WhatsApp number is required*");
    }

    [Fact]
    public void Update_WithValidTemplateId_ShouldUpdateTemplateId()
    {
        var landingPageConfig = LandingPageConfig.Create(
            Guid.NewGuid(),
            1,
            "+5511999999999");

        landingPageConfig.Update(templateId: 3);

        landingPageConfig.TemplateId.Should().Be(3);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var landingPageConfig = LandingPageConfig.Create(
            Guid.NewGuid(),
            1,
            "+5511999999999");

        var newAboutText = "Nova descrição";
        var newOpeningHours = "Seg-Sex: 9h-18h";
        var newInstagram = "@barbearianew";

        landingPageConfig.Update(
            aboutText: newAboutText,
            openingHours: newOpeningHours,
            instagramUrl: newInstagram);

        landingPageConfig.AboutText.Should().Be(newAboutText);
        landingPageConfig.OpeningHours.Should().Be(newOpeningHours);
        landingPageConfig.InstagramUrl.Should().Be(newInstagram);
    }

    [Fact]
    public void Publish_ShouldSetIsPublishedToTrue()
    {
        var landingPageConfig = LandingPageConfig.Create(
            Guid.NewGuid(),
            1,
            "+5511999999999");
        
        landingPageConfig.Unpublish();
        landingPageConfig.Publish();

        landingPageConfig.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_ShouldSetIsPublishedToFalse()
    {
        var landingPageConfig = LandingPageConfig.Create(
            Guid.NewGuid(),
            1,
            "+5511999999999");

        landingPageConfig.Unpublish();

        landingPageConfig.IsPublished.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    public void IsValidTemplate_ShouldReturnCorrectValue(int templateId, bool expected)
    {
        var landingPageConfig = LandingPageConfig.Create(
            Guid.NewGuid(),
            templateId,
            "+5511999999999");

        var result = landingPageConfig.IsValidTemplate();
        result.Should().Be(expected);
    }
}
