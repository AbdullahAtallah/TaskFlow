using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class PackageEntityConfiguration : IEntityTypeConfiguration<PackageEntity>
{
    public void Configure(EntityTypeBuilder<PackageEntity> builder)
    {
        builder.HasKey(package => package.Id);

        builder.Property(package => package.Id).HasMaxLength(FieldLengths.CatalogueId);

        builder.HasData(Seeds.GetPackageSeed());
    }
}
