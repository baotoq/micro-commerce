namespace MicroCommerce.Catalog.API.Infrastructure.Paged
{
    public class Metadata
    {
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? PreviousPage { get; set; }
        public int? NextPage { get; set; }
    }
}
