using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(task => task.Title)
            .HasMaxLength(FieldLengths.TaskTitle)
            .IsRequired();

        // Tasks are soft-deleted, so the database never cascades from a project or a user.
        builder.HasOne(task => task.Project)
            .WithMany(project => project.Tasks)
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(task => task.Assignee)
            .WithMany()
            .HasForeignKey(task => task.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Queries never navigate to the creator: the required join would hide the task once its creator is deleted.
        builder.HasOne(task => task.CreatedBy)
            .WithMany()
            .HasForeignKey(task => task.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // The task list of a project and the "my tasks" card filter on these columns.
        builder.HasIndex(task => new { task.ProjectId, task.Status });
        builder.HasIndex(task => new { task.AssigneeId, task.Status });
    }
}
