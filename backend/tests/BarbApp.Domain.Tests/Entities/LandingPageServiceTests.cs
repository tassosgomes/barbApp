using BarbApp.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BarbApp.Domain.Tests.Entities;

public class LandingPageServiceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateLandingPageService()
    {
        var landingPageConfigId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var displayOrder = 1;

        var landingPageService = LandingPageService.Create(
            landingPageConfigId,
            serviceId,
            displayOrder);

        landingPageService.Should().NotBeNull();
        landingPageService.Id.Should().NotBeEmpty();
        landingPageService.LandingPageConfigId.Should().Be(landingPageConfigId);
        landingPageService.ServiceId.Should().Be(serviceId);
        landingPageService.DisplayOrder.Should().Be(displayOrder);
        landingPageService.IsVisible.Should().BeTrue();
        landingPageService.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyLandingPageConfigId_ShouldThrowException()
    {
        var act = () => LandingPageService.Create(
            Guid.Empty,
            Guid.NewGuid(),
            1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Landing page config ID is required*");
    }

    [Fact]
    public void Create_WithEmptyServiceId_ShouldThrowException()
    {
        var act = () => LandingPageService.Create(
            Guid.NewGuid(),
            Guid.Empty,
            1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Service ID is required*");
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_ShouldThrowException()
    {
        var act = () => LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            -1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Display order must be greater than or equal to zero*");
    }

    [Fact]
    public void UpdateDisplayOrder_WithValidOrder_ShouldUpdateDisplayOrder()
    {
        var landingPageService = LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1);

        landingPageService.UpdateDisplayOrder(5);

        landingPageService.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void UpdateDisplayOrder_WithNegativeOrder_ShouldThrowException()
    {
        var landingPageService = LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1);

        var act = () => landingPageService.UpdateDisplayOrder(-1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Display order must be greater than or equal to zero*");
    }

    [Fact]
    public void Show_ShouldSetIsVisibleToTrue()
    {
        var landingPageService = LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            false);

        landingPageService.Show();

        landingPageService.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void Hide_ShouldSetIsVisibleToFalse()
    {
        var landingPageService = LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1);

        landingPageService.Hide();

        landingPageService.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void ToggleVisibility_ShouldChangeVisibilityState()
    {
        var landingPageService = LandingPageService.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1);

        var initialState = landingPageService.IsVisible;

        landingPageService.ToggleVisibility();

        landingPageService.IsVisible.Should().Be(!initialState);
    }
}
