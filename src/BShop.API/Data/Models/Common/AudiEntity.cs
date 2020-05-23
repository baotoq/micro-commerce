using System;

namespace BShop.API.Data.Models.Common
{
    public abstract class AuditEntity : Entity
    {
        public DateTime Created { get; set; }

        public DateTime? LastModified { get; set; }
    }
}
