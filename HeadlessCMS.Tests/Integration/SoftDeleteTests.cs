using HeadlessCMS.Common.Enums;
using HeadlessCMS.Data;
using HeadlessCMS.Data.Entities;
using HeadlessCMS.Data.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Tests.Integration;

/// <summary>
/// Runs the real DbContext against an in-memory SQLite database. SQLite is relational, so it exercises the
/// same table mapping, owned types, unique indexes and UPDATE statements that Postgres will, without Docker.
/// </summary>
public sealed class SoftDeleteTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SoftDeleteTests()
    {
        _connection.Open(); // an in-memory database lives only as long as its connection
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User NewUser(string email = "ada@example.com") => new()
    {
        Email = new Email(email),
        Name = "Ada",
        PasswordHash = "hash",
        IsActive = true
    };

    [Fact]
    public async Task Soft_deleting_keeps_the_row_and_marks_it_deleted()
    {
        await using (var context = CreateContext())
        {
            context.Users.Add(NewUser());
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var user = await context.Users.SingleAsync();
            context.SoftDelete(user);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var user = await context.Users.IgnoreQueryFilters().SingleAsync();

            Assert.NotNull(user.DeletedAt);
            Assert.Equal("ada@example.com", user.Email.Value); // the owned Email survived
        }
    }

    [Fact]
    public async Task Queries_hide_deleted_rows()
    {
        await using var context = CreateContext();
        var keep = NewUser("keep@example.com");
        var drop = NewUser("drop@example.com");
        context.Users.AddRange(keep, drop);
        await context.SaveChangesAsync();

        context.SoftDelete(drop);
        await context.SaveChangesAsync();

        var visible = await context.Users.ToListAsync();
        var everything = await context.Users.IgnoreQueryFilters().CountAsync();

        Assert.Equal(keep.Id, Assert.Single(visible).Id);
        Assert.Equal(2, everything);
    }

    [Fact]
    public async Task A_deleted_users_email_can_be_registered_again()
    {
        await using var context = CreateContext();
        var first = NewUser();
        context.Users.Add(first);
        await context.SaveChangesAsync();

        context.SoftDelete(first);
        await context.SaveChangesAsync();

        context.Users.Add(NewUser());
        await context.SaveChangesAsync(); // would throw if the deleted row still held the email

        Assert.Single(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task Two_live_users_cannot_share_an_email()
    {
        await using var context = CreateContext();
        context.Users.AddRange(NewUser(), NewUser());

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Audit_logs_keep_their_user_when_the_user_is_deleted()
    {
        await using var context = CreateContext();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            EventType = EventType.UserRegistered,
            EntityId = user.Id,
            EventData = "{}"
        });
        await context.SaveChangesAsync();

        context.SoftDelete(user);
        await context.SaveChangesAsync();

        var log = await context.AuditLogs.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(user.Id, log.UserId);
    }

    [Fact]
    public async Task A_plain_Remove_is_refused_so_rows_are_never_deleted_by_accident()
    {
        await using var context = CreateContext();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Users.Remove(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task HardDelete_really_removes_the_row()
    {
        await using var context = CreateContext();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.HardDelete(user);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.Users.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task A_soft_deleted_row_can_be_found_and_restored()
    {
        await using var context = CreateContext();
        var user = NewUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        context.SoftDelete(user);
        await context.SaveChangesAsync();

        var deleted = await context.Users.IgnoreQueryFilters().SingleAsync();
        deleted.DeletedAt = null;
        await context.SaveChangesAsync();

        Assert.Single(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task Timestamps_are_set_on_add_and_update()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        Guid id;

        await using (var context = CreateContext())
        {
            var user = NewUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            id = user.Id;

            Assert.True(user.CreatedAt >= before);
            Assert.Equal(user.CreatedAt, user.UpdatedAt);
        }

        await using (var context = CreateContext())
        {
            var user = await context.Users.SingleAsync(u => u.Id == id);
            var createdAt = user.CreatedAt;
            user.Name = "Ada Lovelace";
            await context.SaveChangesAsync();

            Assert.Equal(createdAt, user.CreatedAt);
            Assert.True(user.UpdatedAt >= createdAt);
        }
    }
}
