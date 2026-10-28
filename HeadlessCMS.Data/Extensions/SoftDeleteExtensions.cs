using HeadlessCMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadlessCMS.Data;

public static class SoftDeleteExtensions
{
    /// <summary>
    /// SQL that keeps only rows that have not been soft-deleted. Unique indexes use it, so a deleted
    /// row no longer blocks a new row from taking the same email or name.
    /// </summary>
    public const string NotDeletedSql = "\"DeletedAt\" IS NULL";

    /// <summary>
    /// Hides soft-deleted rows from every query on this entity. Call it in each entity's configuration.
    /// Use <c>IgnoreQueryFilters()</c> on a query to see deleted rows as well.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasSoftDeleteFilter<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasQueryFilter(e => e.DeletedAt == null);
        return builder;
    }
}
