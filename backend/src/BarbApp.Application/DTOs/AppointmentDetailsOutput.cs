using BarbApp.Domain.Enums;

namespace BarbApp.Application.DTOs;

public record AppointmentDetailsOutput(
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string ServiceTitle,
    decimal ServicePrice,
    int ServiceDurationMinutes,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    DateTime? CompletedAt
);