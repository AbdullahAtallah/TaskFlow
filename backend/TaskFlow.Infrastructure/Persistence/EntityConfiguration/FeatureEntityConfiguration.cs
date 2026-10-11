using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class FeatureEntityConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    public void Configure(EntityTypeBuilder<FeatureEntity> builder)
    {
        builder.HasKey(feature => feature.Id);

        builder.Property(feature => feature.Id).HasMaxLength(FieldLengths.CatalogueId);
        builder.Property(feature => feature.PackageId).HasMaxLength(FieldLengths.CatalogueId);

        // Enabled by default; EF Core always sends the value, so that a feature can be switched off.
        builder.Property(feature => feature.IsEnabled)
            .HasDefaultValue(true)
            .ValueGeneratedNever();

        builder.HasOne(feature => feature.Package)
            .WithMany()
            .HasForeignKey(feature => feature.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(Seeds.GetFeatureSeed());
    }
}
