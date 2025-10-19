using BarbApp.Domain.Enums;

namespace BarbApp.Application.DTOs;

public record BarberAppointmentOutput(
    Guid Id,
    string CustomerName,
    string ServiceTitle,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status
);