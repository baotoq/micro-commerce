using System;

namespace UnitOfWork.Models
{
    public abstract class AuditEntity : Entity
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
