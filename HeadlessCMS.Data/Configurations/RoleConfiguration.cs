using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasSoftDeleteFilter();

        builder.Property(r => r.Name).IsRequired();

        // Unique among live roles only. This refines the [Index] attribute on Role.
        builder.HasIndex(r => r.Name).IsUnique().HasFilter(SoftDeleteExtensions.NotDeletedSql);
    }
}
