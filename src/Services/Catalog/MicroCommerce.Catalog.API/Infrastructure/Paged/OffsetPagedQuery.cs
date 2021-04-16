namespace MicroCommerce.Catalog.API.Infrastructure.Paged
{
    public abstract class OffsetPagedQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
