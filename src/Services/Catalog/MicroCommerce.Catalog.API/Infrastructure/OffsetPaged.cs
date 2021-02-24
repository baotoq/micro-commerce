using System.Collections.Generic;

namespace MicroCommerce.Catalog.API.Infrastructure
{
    public class OffsetPaged<T>
    {
        public IEnumerable<T> PaginationResult { get; init; }
        
        public Metadata Metadata { get; init; }
    }
}
