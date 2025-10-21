using MediatR;

namespace BarbApp.Domain.Events
{
    public class BarbershopCreatedEvent : INotification
    {
        public Guid BarbershopId { get; set; }
        public string BarbershopName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}