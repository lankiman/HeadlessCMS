using HeadlessCMS.Common.Options;
using HeadlessCMS.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HeadlessCMS.Tests.Unit.Options;

public class AddValidatedOptionsTests
{
    private static IConfiguration Config(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static Dictionary<string, string?> ValidSettings() => new()
    {
        ["Jwt:SigningKey"] = new string('k', 40),
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience",
        ["Security:TwoFactorEncryptionKey"] = Convert.ToBase64String(new byte[32])
    };

    [Fact]
    public void Valid_settings_are_registered_as_IOptions()
    {
        var values = ValidSettings();
        values["Jwt:AccessTokenMinutes"] = "20";
        values["Auth:RefreshTokenDays"] = "7";

        var services = new ServiceCollection().AddValidatedOptions(Config(values));
        using var provider = services.BuildServiceProvider();

        Assert.Equal(20, provider.GetRequiredService<IOptions<JwtOptions>>().Value.AccessTokenMinutes);
        Assert.Equal(7, provider.GetRequiredService<IOptions<AuthOptions>>().Value.RefreshTokenDays);
        Assert.Equal("issuer", provider.GetRequiredService<IOptions<JwtOptions>>().Value.Issuer);
    }

    [Fact]
    public void Starting_with_missing_settings_fails_and_lists_every_problem()
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddValidatedOptions(Config(new())));

        Assert.Contains("Jwt:SigningKey", error.Message);
        Assert.Contains("Jwt:Issuer", error.Message);
        Assert.Contains("Security:TwoFactorEncryptionKey", error.Message);
        Assert.Contains("cannot start", error.Message);
    }

    [Fact]
    public void The_failure_message_never_contains_the_secret_that_was_supplied()
    {
        var values = ValidSettings();
        values["Jwt:SigningKey"] = "too-short-secret";

        var error = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddValidatedOptions(Config(values)));

        Assert.DoesNotContain("too-short-secret", error.Message);
    }
}
