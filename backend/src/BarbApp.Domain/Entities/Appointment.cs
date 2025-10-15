namespace BarbApp.Domain.Entities;

// Placeholder for Appointment entity (to be fully implemented in future tasks)
// This allows IAppointmentRepository to be defined for Task 1.0
public class Appointment
{
    public Guid Id { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string ServiceName { get; private set; }
    public string Status { get; private set; }
    
    private Appointment()
    {
        ServiceName = null!;
        Status = null!;
    }
}
