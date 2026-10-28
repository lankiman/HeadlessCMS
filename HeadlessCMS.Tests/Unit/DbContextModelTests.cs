using HeadlessCMS.Data;
using HeadlessCMS.Data.Entities;
using HeadlessCMS.Data.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Tests.Unit;

/// <summary>
/// Building EF Core's model needs no database connection, so these tests catch mapping mistakes
/// (a missing constructor, a bad relationship) without Postgres running.
/// </summary>
public class DbContextModelTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model-test;Username=test;Password=test")
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void The_model_builds_and_contains_every_entity()
    {
        using var context = CreateContext();
        var model = context.Model;

        Assert.NotNull(model.FindEntityType(typeof(User)));
        Assert.NotNull(model.FindEntityType(typeof(Role)));
        Assert.NotNull(model.FindEntityType(typeof(AuditLog)));
    }

    [Fact]
    public void Tables_are_named_after_their_entities()
    {
        using var context = CreateContext();
        var model = context.Model;

        Assert.Equal("Users", model.FindEntityType(typeof(User))!.GetTableName());
        Assert.Equal("Roles", model.FindEntityType(typeof(Role))!.GetTableName());
        Assert.Equal("AuditLogs", model.FindEntityType(typeof(AuditLog))!.GetTableName());
    }

    [Fact]
    public void Email_lives_in_the_users_table_and_is_unique()
    {
        using var context = CreateContext();
        var emailType = context.Model.FindEntityType(typeof(User))!
            .FindNavigation(nameof(User.Email))!.TargetEntityType;

        Assert.Equal("Users", emailType.GetTableName());
        Assert.Contains(emailType.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == nameof(Email.Value));
    }

    [Fact]
    public void Event_types_are_stored_as_strings()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(AuditLog))!.FindProperty(nameof(AuditLog.EventType))!;

        Assert.Equal(typeof(string), property.GetProviderClrType());
    }

    [Fact]
    public void Role_names_are_unique()
    {
        using var context = CreateContext();
        var role = context.Model.FindEntityType(typeof(Role))!;

        Assert.Contains(role.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == nameof(Role.Name));
    }

    [Fact]
    public void Every_BaseEntity_hides_soft_deleted_rows()
    {
        using var context = CreateContext();

        var entities = context.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        Assert.NotEmpty(entities);
        Assert.All(entities, t => Assert.True(t.GetQueryFilter() is not null, $"{t.ClrType.Name} has no soft-delete query filter."));
    }

    [Fact]
    public void Unique_indexes_ignore_deleted_rows()
    {
        using var context = CreateContext();
        var emailIndex = context.Model.FindEntityType(typeof(User))!
            .FindNavigation(nameof(User.Email))!.TargetEntityType
            .GetIndexes().Single(i => i.IsUnique);
        var roleIndex = context.Model.FindEntityType(typeof(Role))!
            .GetIndexes().Single(i => i.IsUnique);

        Assert.Equal(SoftDeleteExtensions.NotDeletedSql, emailIndex.GetFilter());
        Assert.Equal(SoftDeleteExtensions.NotDeletedSql, roleIndex.GetFilter());
    }
}
