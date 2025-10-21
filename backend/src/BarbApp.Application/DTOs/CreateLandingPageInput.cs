namespace BarbApp.Application.DTOs;

/// <summary>
/// Input para criação de landing page
/// </summary>
public record CreateLandingPageInput(
    Guid BarbershopId,
    int TemplateId,
    string? LogoUrl,
    string? AboutText,
    string? OpeningHours,
    string? InstagramUrl,
    string? FacebookUrl,
    string WhatsappNumber,
    List<ServiceDisplayInput>? Services
);
