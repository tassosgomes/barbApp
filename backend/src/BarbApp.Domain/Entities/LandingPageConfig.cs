namespace BarbApp.Domain.Entities;

/// <summary>
/// Configuração da landing page de uma barbearia
/// </summary>
public class LandingPageConfig
{
    private const int MaxAboutTextLength = 2000;
    private const int MaxOpeningHoursLength = 500;
    private const int MaxUrlLength = 500;
    private const int MaxSocialMediaUrlLength = 255;
    private const int MaxWhatsappLength = 20;
    private const int MinTemplateId = 1;
    private const int MaxTemplateId = 5;

    public Guid Id { get; private set; }
    public Guid BarbershopId { get; private set; }
    public int TemplateId { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? AboutText { get; private set; }
    public string? OpeningHours { get; private set; }
    public string? InstagramUrl { get; private set; }
    public string? FacebookUrl { get; private set; }
    public string WhatsappNumber { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Barbershop Barbershop { get; private set; } = null!;
    public virtual ICollection<LandingPageService> Services { get; private set; } = new List<LandingPageService>();

    private LandingPageConfig()
    {
        WhatsappNumber = null!;
    }

    public static LandingPageConfig Create(
        Guid barbershopId,
        int templateId,
        string whatsappNumber,
        string? logoUrl = null,
        string? aboutText = null,
        string? openingHours = null,
        string? instagramUrl = null,
        string? facebookUrl = null)
    {
        if (barbershopId == Guid.Empty)
            throw new ArgumentException("Barbershop ID is required", nameof(barbershopId));
        
        if (templateId < MinTemplateId || templateId > MaxTemplateId)
            throw new ArgumentException($"Template ID must be between {MinTemplateId} and {MaxTemplateId}", nameof(templateId));
        
        if (string.IsNullOrWhiteSpace(whatsappNumber))
            throw new ArgumentException("WhatsApp number is required", nameof(whatsappNumber));
        
        if (whatsappNumber.Length > MaxWhatsappLength)
            throw new ArgumentException($"WhatsApp number must not exceed {MaxWhatsappLength} characters", nameof(whatsappNumber));
        
        if (logoUrl?.Length > MaxUrlLength)
            throw new ArgumentException($"Logo URL must not exceed {MaxUrlLength} characters", nameof(logoUrl));
        
        if (aboutText?.Length > MaxAboutTextLength)
            throw new ArgumentException($"About text must not exceed {MaxAboutTextLength} characters", nameof(aboutText));
        
        if (openingHours?.Length > MaxOpeningHoursLength)
            throw new ArgumentException($"Opening hours must not exceed {MaxOpeningHoursLength} characters", nameof(openingHours));
        
        if (instagramUrl?.Length > MaxSocialMediaUrlLength)
            throw new ArgumentException($"Instagram URL must not exceed {MaxSocialMediaUrlLength} characters", nameof(instagramUrl));
        
        if (facebookUrl?.Length > MaxSocialMediaUrlLength)
            throw new ArgumentException($"Facebook URL must not exceed {MaxSocialMediaUrlLength} characters", nameof(facebookUrl));

        return new LandingPageConfig
        {
            Id = Guid.NewGuid(),
            BarbershopId = barbershopId,
            TemplateId = templateId,
            WhatsappNumber = whatsappNumber.Trim(),
            LogoUrl = logoUrl?.Trim(),
            AboutText = aboutText?.Trim(),
            OpeningHours = openingHours?.Trim(),
            InstagramUrl = instagramUrl?.Trim(),
            FacebookUrl = facebookUrl?.Trim(),
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        int? templateId = null,
        string? logoUrl = null,
        string? aboutText = null,
        string? openingHours = null,
        string? instagramUrl = null,
        string? facebookUrl = null,
        string? whatsappNumber = null)
    {
        if (templateId.HasValue)
        {
            if (templateId.Value < MinTemplateId || templateId.Value > MaxTemplateId)
                throw new ArgumentException($"Template ID must be between {MinTemplateId} and {MaxTemplateId}", nameof(templateId));
            TemplateId = templateId.Value;
        }

        if (whatsappNumber != null)
        {
            if (string.IsNullOrWhiteSpace(whatsappNumber))
                throw new ArgumentException("WhatsApp number cannot be empty", nameof(whatsappNumber));
            if (whatsappNumber.Length > MaxWhatsappLength)
                throw new ArgumentException($"WhatsApp number must not exceed {MaxWhatsappLength} characters", nameof(whatsappNumber));
            WhatsappNumber = whatsappNumber.Trim();
        }

        if (logoUrl != null)
        {
            if (logoUrl.Length > MaxUrlLength)
                throw new ArgumentException($"Logo URL must not exceed {MaxUrlLength} characters", nameof(logoUrl));
            LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        }

        if (aboutText != null)
        {
            if (aboutText.Length > MaxAboutTextLength)
                throw new ArgumentException($"About text must not exceed {MaxAboutTextLength} characters", nameof(aboutText));
            AboutText = string.IsNullOrWhiteSpace(aboutText) ? null : aboutText.Trim();
        }

        if (openingHours != null)
        {
            if (openingHours.Length > MaxOpeningHoursLength)
                throw new ArgumentException($"Opening hours must not exceed {MaxOpeningHoursLength} characters", nameof(openingHours));
            OpeningHours = string.IsNullOrWhiteSpace(openingHours) ? null : openingHours.Trim();
        }

        if (instagramUrl != null)
        {
            if (instagramUrl.Length > MaxSocialMediaUrlLength)
                throw new ArgumentException($"Instagram URL must not exceed {MaxSocialMediaUrlLength} characters", nameof(instagramUrl));
            InstagramUrl = string.IsNullOrWhiteSpace(instagramUrl) ? null : instagramUrl.Trim();
        }

        if (facebookUrl != null)
        {
            if (facebookUrl.Length > MaxSocialMediaUrlLength)
                throw new ArgumentException($"Facebook URL must not exceed {MaxSocialMediaUrlLength} characters", nameof(facebookUrl));
            FacebookUrl = string.IsNullOrWhiteSpace(facebookUrl) ? null : facebookUrl.Trim();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValidTemplate()
    {
        return TemplateId >= MinTemplateId && TemplateId <= MaxTemplateId;
    }
}
