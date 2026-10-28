using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        // Enums are stored as strings.
        builder.Property(a => a.EventType).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.EventData).IsRequired();

        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.UserId);

        // The user is optional (system events have none); deleting a user keeps their audit trail.
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
