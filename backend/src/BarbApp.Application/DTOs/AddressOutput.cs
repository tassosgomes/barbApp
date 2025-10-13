namespace BarbApp.Application.DTOs;

public record AddressOutput(
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);