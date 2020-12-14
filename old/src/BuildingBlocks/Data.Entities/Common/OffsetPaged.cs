using System.Collections.Generic;

namespace Data.Entities.Common
{
    public class OffsetPaged<T>
    {
        public List<T> Data { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int? PreviousPage { get; set; }
        public int? NextPage { get; set; }
    }
}
