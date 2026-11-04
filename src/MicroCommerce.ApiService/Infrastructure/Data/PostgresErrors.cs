using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.ApiService.Infrastructure.Data;

internal static class PostgresErrors
{
    /// <summary>The insert hit a primary key or unique constraint: the row is already there.</summary>
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
