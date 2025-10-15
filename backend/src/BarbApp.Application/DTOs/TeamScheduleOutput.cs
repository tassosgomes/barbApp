namespace BarbApp.Application.DTOs;

public record TeamScheduleOutput(
    List<AppointmentOutput> Appointments
);