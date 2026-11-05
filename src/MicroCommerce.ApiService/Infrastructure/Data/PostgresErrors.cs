using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.ApiService.Infrastructure.Data;

public static class PostgresErrors
{
    /// <summary>The insert hit a primary key or unique constraint: the row is already there.</summary>
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    /// <summary>The name of the unique index or constraint the insert or update hit, if it hit one.</summary>
    public static string? UniqueViolationConstraint(this DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } e ? e.ConstraintName : null;
}
