namespace BarbApp.Application.DTOs;

public record CreateBarberInput(
    string Name,
    string Email,
    string Password,
    string Phone,
    List<Guid> ServiceIds
);