namespace BarbApp.Application.DTOs;

/// <summary>
/// Output com configuração completa da landing page para administração
/// </summary>
public record LandingPageConfigOutput(
    Guid Id,
    Guid BarbershopId,
    int TemplateId,
    string? LogoUrl,
    string? AboutText,
    string? OpeningHours,
    string? InstagramUrl,
    string? FacebookUrl,
    string WhatsappNumber,
    bool IsPublished,
    DateTime UpdatedAt,
    BarbershopBasicInfoOutput? Barbershop,
    List<LandingPageServiceOutput> Services
);

/// <summary>
/// Output com informações básicas da barbearia
/// </summary>
public record BarbershopBasicInfoOutput(
    Guid Id,
    string Name,
    string Code,
    string Address
);

/// <summary>
/// Output com informações do serviço na landing page
/// </summary>
public record LandingPageServiceOutput(
    Guid ServiceId,
    string ServiceName,
    string? Description,
    int DurationMinutes,
    decimal Price,
    int DisplayOrder,
    bool IsVisible
);
