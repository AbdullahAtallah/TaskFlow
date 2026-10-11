using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class TeamEntityConfiguration : IEntityTypeConfiguration<TeamEntity>
{
    public void Configure(EntityTypeBuilder<TeamEntity> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(team => team.Name)
            .HasMaxLength(FieldLengths.TeamName)
            .IsRequired();

        builder.HasIndex(team => team.Name).IsUniqueAmongActiveRows();
    }
}
