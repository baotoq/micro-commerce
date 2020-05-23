using System.Collections.Generic;
using BShop.API.Data.Models.Common;
using BShop.API.Data.Models.Enums;

namespace BShop.API.Data.Models
{
    public class Review : AuditEntity
    {
        public long UserId { get; set; }

        public string? Title { get; set; }

        public string? Comment { get; set; }

        public int Rating { get; set; }

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public IList<Reply> Replies { get; protected set; } = new List<Reply>();

        public long ProductId { get; set; }

        public Product? Product { get; set; }
    }
}
