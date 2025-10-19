namespace BarbApp.Domain.Exceptions;

public class InvalidAppointmentStatusTransitionException : DomainException
{
    public InvalidAppointmentStatusTransitionException(string currentStatus, string attemptedAction)
        : base($"Cannot {attemptedAction} appointment with status '{currentStatus}'")
    {
    }
}
