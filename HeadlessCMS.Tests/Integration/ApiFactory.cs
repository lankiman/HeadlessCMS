using Microsoft.AspNetCore.Mvc.Testing;

namespace HeadlessCMS.Tests.Integration;

/// <summary>
/// Starts the whole API in memory. Every database points at a port nothing listens on, so a test can
/// prove how the app behaves when its services are down, without Docker.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private static readonly Dictionary<string, string> Settings = new()
    {
        ["ConnectionStrings__DefaultConnection"] = "Host=localhost;Port=1;Database=test;Username=test;Password=test;Timeout=2;Command Timeout=2",
        ["ConnectionStrings__Redis"] = "localhost:1,abortConnect=false,connectTimeout=500,syncTimeout=1000",
        ["ConnectionStrings__Mongo"] = "mongodb://localhost:1/?serverSelectionTimeoutMS=1000&connectTimeoutMS=500",
        ["Mongo__Database"] = "test",
        ["Jwt__SigningKey"] = "test-signing-key-test-signing-key-0123456789",
        ["Security__TwoFactorEncryptionKey"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        ["Database__MigrateOnStartup"] = "false"
    };

    public ApiFactory()
    {
        // Real environment variables beat appsettings and any .env file, so a developer's own settings
        // can never leak into these tests.
        foreach (var (key, value) in Settings)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        foreach (var key in Settings.Keys)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }
}
