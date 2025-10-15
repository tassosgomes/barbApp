namespace BarbApp.Application.DTOs;

public record AppointmentOutput(
    Guid Id,
    Guid BarberId,
    string BarberName,
    Guid CustomerId,
    string CustomerName,
    DateTime StartTime,
    DateTime EndTime,
    string ServiceName,
    string Status
);