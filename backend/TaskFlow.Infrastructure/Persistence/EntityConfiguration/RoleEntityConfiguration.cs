using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(role => role.Name)
            .HasMaxLength(FieldLengths.RoleName)
            .IsRequired();

        builder.Property(role => role.Description).HasMaxLength(FieldLengths.RoleDescription);

        builder.HasIndex(role => role.Name).IsUniqueAmongActiveRows();

        builder.HasData(Seeds.GetRoleSeed());
    }
}
