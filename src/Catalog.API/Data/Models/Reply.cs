using Catalog.API.Data.Models.Common;
using Catalog.API.Data.Models.Enums;

namespace Catalog.API.Data.Models
{
    public class Reply : AuditEntity
    {
        public Reply()
        {
            Status = ReplyStatus.Pending;
        }

        public long UserId { get; set; }

        public string? Comment { get; set; }

        public ReplyStatus Status { get; set; }

        public long ReviewId { get; set; }

        public Review? Review { get; set; }
    }
}
