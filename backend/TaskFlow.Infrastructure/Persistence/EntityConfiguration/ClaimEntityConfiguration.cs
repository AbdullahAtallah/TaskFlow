using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class ClaimEntityConfiguration : IEntityTypeConfiguration<ClaimEntity>
{
    public void Configure(EntityTypeBuilder<ClaimEntity> builder)
    {
        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id).HasMaxLength(FieldLengths.CatalogueId);
        builder.Property(claim => claim.PackageId).HasMaxLength(FieldLengths.CatalogueId);
        builder.Property(claim => claim.FeatureId).HasMaxLength(FieldLengths.CatalogueId);

        // No cascades here: SQL Server forbids a second cascade path from Packages; a migration removes claims first.
        builder.HasOne(claim => claim.Package)
            .WithMany()
            .HasForeignKey(claim => claim.PackageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(claim => claim.Feature)
            .WithMany()
            .HasForeignKey(claim => claim.FeatureId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasData(Seeds.GetClaimSeed());
    }
}
