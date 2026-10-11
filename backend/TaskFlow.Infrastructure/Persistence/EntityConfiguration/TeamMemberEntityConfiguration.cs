using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities.Teams;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class TeamMemberEntityConfiguration : IEntityTypeConfiguration<TeamMemberEntity>
{
    public void Configure(EntityTypeBuilder<TeamMemberEntity> builder)
    {
        builder.HasKey(member => new { member.TeamId, member.UserId });

        builder.HasOne(member => member.Team)
            .WithMany(team => team.Members)
            .HasForeignKey(member => member.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany(user => user.TeamMemberships)
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
