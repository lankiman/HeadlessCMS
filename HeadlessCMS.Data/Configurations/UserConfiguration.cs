using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasSoftDeleteFilter();

        // Email is a value object stored as two columns of the Users table.
        // Unique among live users only, so a deleted user's email can be registered again.
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(255).IsRequired();
            email.Property(e => e.IsVerified).HasColumnName("EmailVerified");
            email.HasIndex(e => e.Value).IsUnique().HasFilter(SoftDeleteExtensions.NotDeletedSql);
        });
        builder.Navigation(u => u.Email).IsRequired();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Name).IsRequired();
    }
}
