using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Common.Extensions;

public static class RowVersionExtensions
{
    /// <summary>
    /// Refuses the save when the record changed after the user's form loaded it. A change between this check and the
    /// save is caught too: SQL Server compares the row version again when the row is updated.
    /// </summary>
    public static void EnsureUnchangedSince(this BaseEntity entity, byte[] rowVersion)
    {
        if (!entity.RowVersion.AsSpan().SequenceEqual(rowVersion))
        {
            throw new ConcurrencyConflictException();
        }
    }
}
