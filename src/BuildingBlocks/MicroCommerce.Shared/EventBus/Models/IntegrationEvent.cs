using System;

namespace MicroCommerce.Shared.EventBus.Models
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; }

        public DateTime CreatedAt { get; }
    }
}
