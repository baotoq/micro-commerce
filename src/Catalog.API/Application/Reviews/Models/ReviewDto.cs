using System;
using Catalog.API.Data.Models.Enums;

namespace Catalog.API.Application.Reviews.Models
{
    public class ReviewDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public string ProductName { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
