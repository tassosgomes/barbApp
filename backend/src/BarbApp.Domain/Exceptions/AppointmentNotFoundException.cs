namespace BarbApp.Domain.Exceptions;

public class AppointmentNotFoundException : NotFoundException
{
    public AppointmentNotFoundException(Guid appointmentId)
        : base($"Appointment with ID '{appointmentId}' was not found")
    {
    }
}
