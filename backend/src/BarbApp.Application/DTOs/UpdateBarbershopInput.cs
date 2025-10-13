namespace BarbApp.Application.DTOs;

public record UpdateBarbershopInput(
    Guid Id,
    string Name,
    string Phone,
    string OwnerName,
    string Email,
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);