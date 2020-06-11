using System;

namespace UnitOfWork.Models
{
    public abstract class AuditEntity : Entity
    {
        public DateTime CreatedDate { get; set; }

        public DateTime? LastModified { get; set; }
    }
}
