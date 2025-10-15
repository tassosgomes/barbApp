namespace BarbApp.Application.DTOs;

public record BarbershopServiceOutput(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    bool IsActive
);