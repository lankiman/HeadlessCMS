using HeadlessCMS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace HeadlessCMS.Tests.Unit;

/// <summary>
/// Checks how MongoDB is registered from configuration. MongoClient connects lazily,
/// so none of these tests needs a running MongoDB.
/// </summary>
public class MongoWiringTests
{
    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "HeadlessCMS.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static ServiceProvider Build(Dictionary<string, string?> settings, string environment = "Development")
    {
        settings.TryAdd("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test");

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataServices(configuration, new TestEnvironment(environment));

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Uses_the_connection_string_and_database_name_from_configuration()
    {
        using var provider = Build(new()
        {
            ["ConnectionStrings:Mongo"] = "mongodb://cms-host:27017",
            ["Mongo:Database"] = "cms-test"
        });

        var client = provider.GetRequiredService<IMongoClient>();
        var database = provider.GetRequiredService<IMongoDatabase>();

        Assert.Equal("cms-host", client.Settings.Server.Host);
        Assert.Equal("cms-test", database.DatabaseNamespace.DatabaseName);
    }

    [Fact]
    public void Falls_back_to_the_database_named_in_the_connection_string()
    {
        using var provider = Build(new() { ["ConnectionStrings:Mongo"] = "mongodb://cms-host:27017/from-url" });

        Assert.Equal("from-url", provider.GetRequiredService<IMongoDatabase>().DatabaseNamespace.DatabaseName);
    }

    [Fact]
    public void Falls_back_to_a_default_database_name()
    {
        using var provider = Build(new() { ["ConnectionStrings:Mongo"] = "mongodb://cms-host:27017" });

        Assert.Equal("headlesscms", provider.GetRequiredService<IMongoDatabase>().DatabaseNamespace.DatabaseName);
    }

    [Fact]
    public void Development_falls_back_to_localhost()
    {
        using var provider = Build(new());

        Assert.Equal("localhost", provider.GetRequiredService<IMongoClient>().Settings.Server.Host);
    }

    [Fact]
    public void Other_environments_must_set_the_connection_string()
        => Assert.Throws<InvalidOperationException>(() => Build(new(), environment: "Production"));

    [Fact]
    public void One_client_is_shared_by_everything()
    {
        using var provider = Build(new() { ["ConnectionStrings:Mongo"] = "mongodb://cms-host:27017" });

        Assert.Same(provider.GetRequiredService<IMongoClient>(), provider.GetRequiredService<IMongoClient>());
    }
}
