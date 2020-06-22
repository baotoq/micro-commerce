using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Common
{
    public class OffsetPaged<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public OffsetPaged(IEnumerable<T> collection) : base(collection)
        {
        }
    }

    public static class OffsetPagedResultExtensions
    {
        public static OffsetPaged<T> ToPaged<T>(this IQueryable<T> source, int page, int pageSize)
        {
            var totalCount = source.Count();
            var totalPage = (double)totalCount / pageSize;

            var result = new OffsetPaged<T>(
                source
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList())
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalPage)
            };

            return result;
        }

        public static async Task<OffsetPaged<T>> ToPagedAsync<T>(this IQueryable<T> source, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var totalCount = await source.CountAsync(cancellationToken);
            var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paged = new OffsetPaged<T>(result)
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = pageCount
            };

            return paged;
        }
    }
}
