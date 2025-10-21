namespace BarbApp.Application.DTOs;

/// <summary>
/// Input para atualizar configuração da landing page
/// </summary>
public record UpdateLandingPageInput(
    int? TemplateId,
    string? LogoUrl,
    string? AboutText,
    string? OpeningHours,
    string? InstagramUrl,
    string? FacebookUrl,
    string? WhatsappNumber,
    List<ServiceDisplayInput>? Services
);

/// <summary>
/// Input para configuração de exibição de serviço
/// </summary>
public record ServiceDisplayInput(
    Guid ServiceId,
    int DisplayOrder,
    bool IsVisible
);
