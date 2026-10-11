using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Infrastructure.Persistence.Extensions;

internal static class DbUpdateExceptionExtensions
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation };
    }
}
