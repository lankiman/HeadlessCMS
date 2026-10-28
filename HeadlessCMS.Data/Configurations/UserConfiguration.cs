using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Email is a value object stored as two columns of the Users table.
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(255).IsRequired();
            email.Property(e => e.IsVerified).HasColumnName("EmailVerified");
            email.HasIndex(e => e.Value).IsUnique();
        });
        builder.Navigation(u => u.Email).IsRequired();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Name).IsRequired();
    }
}
