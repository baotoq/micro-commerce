using System.Collections.Generic;
using Catalog.API.Data.Models.Enums;
using UnitOfWork.Models;

namespace Catalog.API.Data.Models
{
    public class Review : AuditEntity
    {
        public long UserId { get; set; }

        public string? Title { get; set; }

        public string? Comment { get; set; }

        public int Rating { get; set; }

        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Pending;

        public IList<Reply> Replies { get; protected set; } = new List<Reply>();

        public long ProductId { get; set; }

        public Product? Product { get; set; }
    }
}
