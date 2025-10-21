namespace BarbApp.Domain.Entities;

/// <summary>
/// Relação entre landing page e serviços exibidos
/// </summary>
public class LandingPageService
{
    public Guid Id { get; private set; }
    public Guid LandingPageConfigId { get; private set; }
    public Guid ServiceId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual LandingPageConfig LandingPageConfig { get; private set; } = null!;
    public virtual BarbershopService Service { get; private set; } = null!;

    private LandingPageService()
    {
    }

    public static LandingPageService Create(
        Guid landingPageConfigId,
        Guid serviceId,
        int displayOrder,
        bool isVisible = true)
    {
        if (landingPageConfigId == Guid.Empty)
            throw new ArgumentException("Landing page config ID is required", nameof(landingPageConfigId));
        
        if (serviceId == Guid.Empty)
            throw new ArgumentException("Service ID is required", nameof(serviceId));
        
        if (displayOrder < 0)
            throw new ArgumentException("Display order must be greater than or equal to zero", nameof(displayOrder));

        return new LandingPageService
        {
            Id = Guid.NewGuid(),
            LandingPageConfigId = landingPageConfigId,
            ServiceId = serviceId,
            DisplayOrder = displayOrder,
            IsVisible = isVisible,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentException("Display order must be greater than or equal to zero", nameof(displayOrder));
        DisplayOrder = displayOrder;
    }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }
}
