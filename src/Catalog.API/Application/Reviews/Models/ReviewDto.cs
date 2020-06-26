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
        public ReviewStatus ReviewStatus { get; set; }
        public long CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
