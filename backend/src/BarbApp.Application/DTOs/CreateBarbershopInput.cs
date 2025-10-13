namespace BarbApp.Application.DTOs;

public record CreateBarbershopInput(
    string Name,
    string Document,
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