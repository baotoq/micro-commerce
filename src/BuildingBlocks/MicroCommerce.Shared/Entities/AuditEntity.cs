using System;

namespace MicroCommerce.Shared.Entities
{
    public abstract class AuditEntity : Entity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
