namespace BarbApp.Application.DTOs;

public record CreateBarbershopServiceInput(
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price
);