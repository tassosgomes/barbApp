namespace BarbApp.Application.DTOs;

/// <summary>
/// Output com informações públicas da landing page (sem dados administrativos)
/// </summary>
public record PublicLandingPageOutput(
    BarbershopPublicInfoOutput Barbershop,
    LandingPagePublicInfoOutput LandingPage
);

/// <summary>
/// Output com informações públicas da barbearia
/// </summary>
public record BarbershopPublicInfoOutput(
    Guid Id,
    string Name,
    string Code,
    string Address
);

/// <summary>
/// Output com informações públicas da landing page
/// </summary>
public record LandingPagePublicInfoOutput(
    int TemplateId,
    string? LogoUrl,
    string? AboutText,
    string? OpeningHours,
    string? InstagramUrl,
    string? FacebookUrl,
    string WhatsappNumber,
    List<PublicServiceInfoOutput> Services
);

/// <summary>
/// Output com informações públicas do serviço
/// </summary>
public record PublicServiceInfoOutput(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price
);
