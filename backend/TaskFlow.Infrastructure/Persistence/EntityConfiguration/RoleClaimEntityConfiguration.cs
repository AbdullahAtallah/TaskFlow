using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class RoleClaimEntityConfiguration : IEntityTypeConfiguration<RoleClaimEntity>
{
    public void Configure(EntityTypeBuilder<RoleClaimEntity> builder)
    {
        builder.HasKey(roleClaim => new { roleClaim.RoleId, roleClaim.ClaimId });

        builder.Property(roleClaim => roleClaim.ClaimId).HasMaxLength(FieldLengths.CatalogueId);

        builder.HasOne(roleClaim => roleClaim.Role)
            .WithMany(role => role.RoleClaims)
            .HasForeignKey(roleClaim => roleClaim.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(roleClaim => roleClaim.Claim)
            .WithMany()
            .HasForeignKey(roleClaim => roleClaim.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(Seeds.GetRoleClaimSeed());
    }
}
