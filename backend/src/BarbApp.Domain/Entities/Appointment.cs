using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Entities;

public class Appointment
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties (optional, can be used by EF Core)
    public Barbershop? Barbearia { get; private set; }
    public Barber? Barber { get; private set; }
    public BarbershopService? Service { get; private set; }

    // Computed property for backwards compatibility
    public string ServiceName => Service?.Name ?? string.Empty;
    
    // Status as string for backwards compatibility
    public string StatusString => Status.ToString();

    private Appointment()
    {
        // EF Core constructor
    }

    public static Appointment Create(
        Guid barbeariaId,
        Guid barberId,
        Guid customerId,
        Guid serviceId,
        DateTime startTime,
        DateTime endTime)
    {
        // Validations
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("BarbeariaId is required", nameof(barbeariaId));

        if (barberId == Guid.Empty)
            throw new ArgumentException("BarberId is required", nameof(barberId));

        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required", nameof(customerId));

        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId is required", nameof(serviceId));

        if (startTime == default)
            throw new ArgumentException("StartTime is required", nameof(startTime));

        if (endTime == default)
            throw new ArgumentException("EndTime is required", nameof(endTime));

        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be after StartTime", nameof(endTime));

        var now = DateTime.UtcNow;

        return new Appointment
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            BarberId = barberId,
            CustomerId = customerId,
            ServiceId = serviceId,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidAppointmentStatusTransitionException(Status.ToString(), "confirm");

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStatusTransitionException(Status.ToString(), "cancel");

        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidAppointmentStatusTransitionException(Status.ToString(), "complete");

        if (StartTime > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot complete an appointment that has not started yet.");

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
