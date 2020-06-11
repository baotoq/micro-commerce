using Catalog.API.Data.Models.Enums;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Reply : AuditEntity
    {
        public Reply()
        {
            ReplyStatus = ReplyStatus.Pending;
        }

        public long UserId { get; set; }

        public string? Comment { get; set; }

        public ReplyStatus ReplyStatus { get; set; }

        public long ReviewId { get; set; }

        public Review? Review { get; set; }
    }
}
