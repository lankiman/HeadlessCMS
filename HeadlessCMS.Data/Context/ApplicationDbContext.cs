using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data;

public class ApplicationDbContext : DbContext
{
    private bool _hardDeleteRequested;

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

    /// <summary>
    /// Deletes an entity softly: the row stays and DeletedAt is set, so queries hide it.
    /// Use this instead of Remove. (EF reacts to a row being marked as deleted by clearing related
    /// links, such as an audit log's UserId, before we could turn it into an update. So a soft delete
    /// must never mark the row as deleted in the first place.)
    /// </summary>
    public void SoftDelete(BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            entry.State = EntityState.Unchanged; // start tracking it
        }

        entry.Property(e => e.DeletedAt).CurrentValue = DateTime.UtcNow;
    }

    /// <summary>
    /// Really deletes the row (a purge). Every real delete in the next save is allowed.
    /// Soft-deleted rows are hidden by queries, so load them with IgnoreQueryFilters() first.
    /// </summary>
    public void HardDelete(BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _hardDeleteRequested = true;
        Remove(entity);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            ApplyEntityRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            _hardDeleteRequested = false;
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            ApplyEntityRules();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            _hardDeleteRequested = false;
        }
    }

    // Rules that apply to every BaseEntity on save, so no calling code has to remember them:
    //  - CreatedAt and UpdatedAt are set.
    //  - A plain Remove is refused, so a row is never deleted by accident. Use SoftDelete (or HardDelete).
    // ExecuteDelete and raw SQL skip these rules.
    private void ApplyEntityRules()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
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

                case EntityState.Deleted when !_hardDeleteRequested:
                    throw new InvalidOperationException(
                        $"{entry.Metadata.ClrType.Name} was removed with Remove(), which would delete the row for good. " +
                        "Call SoftDelete(entity) to hide it, or HardDelete(entity) to really delete it.");
            }
        }
    }
}
