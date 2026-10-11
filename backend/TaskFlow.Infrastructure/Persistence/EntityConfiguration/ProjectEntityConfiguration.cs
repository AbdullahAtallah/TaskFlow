using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(project => project.Name)
            .HasMaxLength(FieldLengths.ProjectName)
            .IsRequired();

        // Projects are soft-deleted, so the database never cascades from a team.
        builder.HasOne(project => project.Team)
            .WithMany(team => team.Projects)
            .HasForeignKey(project => project.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
