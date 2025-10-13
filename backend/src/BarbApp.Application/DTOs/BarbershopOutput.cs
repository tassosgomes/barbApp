namespace BarbApp.Application.DTOs;

public record BarbershopOutput(
    Guid Id,
    string Name,
    string Document,
    string Phone,
    string OwnerName,
    string Email,
    string Code,
    bool IsActive,
    AddressOutput Address,
    DateTime CreatedAt,
    DateTime UpdatedAt
);