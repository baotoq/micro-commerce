namespace Catalog.API.ApiControllers.Models
{
    public class PagedDto<T>
    {
        public T Data { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }
    }
}
