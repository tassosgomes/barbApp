namespace BarbApp.Application.DTOs;

public record BarberOutput(
    Guid Id,
    string Name,
    string Email,
    string PhoneFormatted,
    List<BarbershopServiceOutput> Services,
    bool IsActive,
    DateTime CreatedAt
);