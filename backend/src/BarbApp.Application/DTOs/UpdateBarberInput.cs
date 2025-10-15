namespace BarbApp.Application.DTOs;

public record UpdateBarberInput(
    Guid Id,
    string Name,
    string Phone,
    List<Guid> ServiceIds
);