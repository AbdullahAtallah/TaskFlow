using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal static class BaseEntityConfigurationExtensions
{
    /// <summary>The audit columns hold UTC; this marks them as UTC again when they are read back.</summary>
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    /// <summary>The key, the BaseEntity columns and the global query filter that hides soft-deleted rows.</summary>
    public static void ConfigureBaseEntity<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.HasKey(entity => entity.Id);

        // A SQL Server rowversion and a concurrency token: an UPDATE only succeeds while the row is still the version
        // that was loaded.
        builder.Property(entity => entity.RowVersion).IsRowVersion();

        builder.Property(entity => entity.DateTimeCreated)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd()
            .HasConversion(UtcConverter);

        builder.Property(entity => entity.DateTimeModified).HasConversion(UtcConverter);
        builder.Property(entity => entity.DateTimeDeleted).HasConversion(UtcConverter);

        builder.HasQueryFilter(entity => entity.DateTimeDeleted == null);
    }

    /// <summary>A unique index that ignores soft-deleted rows, so a deleted name or e-mail address can be used again.</summary>
    public static IndexBuilder<T> IsUniqueAmongActiveRows<T>(this IndexBuilder<T> indexBuilder)
    {
        return indexBuilder
            .IsUnique()
            .HasFilter("[DateTimeDeleted] IS NULL");
    }
}
