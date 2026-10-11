using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Infrastructure.Persistence.EntityConfiguration;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(user => user.UserName)
            .HasMaxLength(FieldLengths.UserName)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(FieldLengths.DisplayName)
            .IsRequired();

        // DEFAULT 1 in the database; EF Core always sends the value, so that false can be stored too.
        builder.Property(user => user.IsActive)
            .HasDefaultValue(true)
            .ValueGeneratedNever();

        builder.Property(user => user.PasswordHash).HasMaxLength(FieldLengths.PasswordHash);

        // Rows that existed before this column get a stamp of their own.
        builder.Property(user => user.SecurityStamp).HasDefaultValueSql("NEWID()");

        builder.HasIndex(user => user.UserName).IsUniqueAmongActiveRows();
    }
}
