using BShop.API.Data.Models.Common;
using BShop.API.Data.Models.Enums;

namespace BShop.API.Data.Models
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
