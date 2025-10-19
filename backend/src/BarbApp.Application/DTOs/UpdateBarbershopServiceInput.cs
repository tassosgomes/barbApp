namespace BarbApp.Application.DTOs;

public record UpdateBarbershopServiceInput(
    Guid Id,
    string? Name,
    string? Description,
    int? DurationMinutes,
    decimal? Price
);