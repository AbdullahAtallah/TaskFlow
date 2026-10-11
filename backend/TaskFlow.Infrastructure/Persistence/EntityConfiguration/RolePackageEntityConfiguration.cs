using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class RolePackageEntityConfiguration : IEntityTypeConfiguration<RolePackageEntity>
{
    public void Configure(EntityTypeBuilder<RolePackageEntity> builder)
    {
        builder.HasKey(rolePackage => new { rolePackage.RoleId, rolePackage.PackageId });

        builder.Property(rolePackage => rolePackage.PackageId).HasMaxLength(FieldLengths.CatalogueId);

        builder.HasOne(rolePackage => rolePackage.Role)
            .WithMany(role => role.RolePackages)
            .HasForeignKey(rolePackage => rolePackage.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rolePackage => rolePackage.Package)
            .WithMany()
            .HasForeignKey(rolePackage => rolePackage.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(Seeds.GetRolePackageSeed());
    }
}
