using HeadlessCMS.Data;
using HeadlessCMS.Data.Health;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using StackExchange.Redis;

namespace HeadlessCMS.Tests.Unit.Health;

public class HealthCheckTests
{
    [Fact]
    public async Task Postgres_check_is_healthy_when_the_database_answers()
    {
        // SQLite stands in for PostgreSQL: the check only asks "can I connect?"
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var context = new ApplicationDbContext(options);

        var result = await new PostgresHealthCheck(context).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Postgres_check_is_unhealthy_when_nothing_is_listening()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=1;Database=x;Username=x;Password=x;Timeout=2")
            .Options;
        await using var context = new ApplicationDbContext(options);

        var result = await new PostgresHealthCheck(context).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task Mongo_check_is_unhealthy_when_nothing_is_listening()
    {
        var client = new MongoClient("mongodb://localhost:1/?serverSelectionTimeoutMS=500&connectTimeoutMS=500");

        var result = await new MongoHealthCheck(client.GetDatabase("x")).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task Redis_check_is_unhealthy_when_nothing_is_listening()
    {
        await using var redis = await ConnectionMultiplexer.ConnectAsync("localhost:1,abortConnect=false,connectTimeout=500,syncTimeout=500");

        var result = await new RedisHealthCheck(redis).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
