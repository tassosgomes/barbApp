namespace BarbApp.Application.DTOs;

public record BarberScheduleOutput(
    DateTime Date,
    Guid BarberId,
    string BarberName,
    List<BarberAppointmentOutput> Appointments
);