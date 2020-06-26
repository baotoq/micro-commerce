using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UnitOfWork.Common
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

    public static class OffsetPagedExtensions
    {
        public static OffsetPaged<T> ToPaged<T>(this IQueryable<T> source, int page, int pageSize)
        {
            var totalCount = source.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paged = new OffsetPaged<T>
            {
                Data = result,
                CurrentPage = page,
                PreviousPage = page - 1 <= 0 ? (int?)null : page - 1,
                NextPage = page + 1 > totalPages ? (int?)null : page + 1,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return paged;
        }

        public static async Task<OffsetPaged<T>> ToPagedAsync<T>(this IQueryable<T> source, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var totalCount = await source.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paged = new OffsetPaged<T>
            {
                Data = result,
                CurrentPage = page,
                PreviousPage = page - 1 <= 0 ? (int?)null : page - 1,
                NextPage = page + 1 > totalPages ? (int?)null : page + 1,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return paged;
        }
    }
}
