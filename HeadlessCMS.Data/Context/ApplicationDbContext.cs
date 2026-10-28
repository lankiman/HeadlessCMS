using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HeadlessCMS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scans HeadlessCMS.Data.dll for all IEntityTypeConfiguration<T> files automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyEntityRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Rules that apply to every BaseEntity on save, so no calling code has to remember them:
    //  - CreatedAt and UpdatedAt are set.
    //  - Remove() does not delete the row. It sets DeletedAt (a "soft delete"), and queries hide such rows.
    //    Query with IgnoreQueryFilters() to see them. ExecuteDelete and raw SQL skip this rule.
    private void ApplyEntityRules()
    {
        var now = DateTime.UtcNow;

        // ToList: changing an entry's state while enumerating the tracker is not allowed.
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    break;

                case EntityState.Deleted:
                    SoftDelete(entry, now);
                    break;
            }
        }
    }

    private static void SoftDelete(EntityEntry<BaseEntity> entry, DateTime now)
    {
        // Deleting an owner also marks its owned objects (such as Email) as deleted.
        // Put them back, or EF would try to blank out their columns.
        foreach (var reference in entry.References)
        {
            if (reference.TargetEntry is { } owned && owned.Metadata.IsOwned())
            {
                owned.State = EntityState.Unchanged;
            }
        }

        entry.State = EntityState.Unchanged;
        entry.Property(e => e.DeletedAt).CurrentValue = now;
        entry.Property(e => e.UpdatedAt).CurrentValue = now;
    }
}
