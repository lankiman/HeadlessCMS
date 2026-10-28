using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasSoftDeleteFilter();

        // Enums are stored as strings.
        builder.Property(a => a.EventType).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.EventData).IsRequired();

        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.UserId);

        // The user is optional (system events have none). Users are soft-deleted, never removed, so the
        // link must never be cleared: Restrict keeps UserId untouched when a user is deleted.
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
