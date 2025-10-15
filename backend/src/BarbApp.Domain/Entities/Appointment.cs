namespace BarbApp.Domain.Entities;

// Placeholder for Appointment entity (to be fully implemented in future tasks)
// This allows IAppointmentRepository to be defined for Task 1.0
public class Appointment
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string ServiceName { get; private set; }
    public string Status { get; private set; }

    private Appointment()
    {
        ServiceName = null!;
        Status = null!;
    }

    public Appointment(
        Guid barbeariaId,
        Guid customerId,
        Guid barberId,
        Guid serviceId,
        DateTime startTime,
        DateTime endTime,
        string status)
    {
        Id = Guid.NewGuid();
        BarbeariaId = barbeariaId;
        CustomerId = customerId;
        BarberId = barberId;
        ServiceId = serviceId;
        StartTime = startTime;
        EndTime = endTime;
        ServiceName = string.Empty; // Will be populated from service
        Status = status;
    }

    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
    }
}
