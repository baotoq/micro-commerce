using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork.EF.Common
{
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
