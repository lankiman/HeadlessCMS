using HeadlessCMS.Common.Options;

namespace HeadlessCMS.Tests.Unit.Options;

public class OptionsValidatorTests
{
    private static JwtOptions ValidJwt() => new()
    {
        SigningKey = new string('k', 32),
        Issuer = "issuer",
        Audience = "audience"
    };

    private static SecurityOptions ValidSecurity() => new()
    {
        TwoFactorEncryptionKey = Convert.ToBase64String(new byte[32])
    };

    [Fact]
    public void Valid_settings_have_no_errors()
    {
        Assert.Empty(JwtOptionsValidator.Validate(ValidJwt()));
        Assert.Empty(AuthOptionsValidator.Validate(new AuthOptions()));
        Assert.Empty(SecurityOptionsValidator.Validate(ValidSecurity()));
    }

    [Fact]
    public void Defaults_match_the_plan()
    {
        Assert.Equal(15, new JwtOptions().AccessTokenMinutes);
        Assert.Equal(30, new AuthOptions().RefreshTokenDays);
        Assert.Equal(5, new AuthOptions().PendingTwoFactorMinutes);
    }

    [Fact]
    public void A_missing_signing_key_is_reported()
    {
        var options = ValidJwt();
        options.SigningKey = "";

        Assert.Contains(JwtOptionsValidator.Validate(options), e => e.Contains("Jwt:SigningKey is required"));
    }

    [Fact]
    public void A_signing_key_under_32_bytes_is_rejected()
    {
        var options = ValidJwt();
        options.SigningKey = new string('k', 31);

        Assert.Contains(JwtOptionsValidator.Validate(options), e => e.Contains("too short"));
    }

    [Fact]
    public void Issuer_and_audience_are_required()
    {
        var options = ValidJwt();
        options.Issuer = " ";
        options.Audience = "";

        var errors = JwtOptionsValidator.Validate(options);

        Assert.Contains(errors, e => e.Contains("Jwt:Issuer"));
        Assert.Contains(errors, e => e.Contains("Jwt:Audience"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1441)]
    public void Access_token_minutes_must_be_in_range(int minutes)
    {
        var options = ValidJwt();
        options.AccessTokenMinutes = minutes;

        Assert.Contains(JwtOptionsValidator.Validate(options), e => e.Contains("AccessTokenMinutes"));
    }

    [Fact]
    public void Auth_numbers_must_be_in_range()
    {
        var errors = AuthOptionsValidator.Validate(new AuthOptions
        {
            RefreshTokenDays = 0,
            PendingTwoFactorMinutes = 61,
            MaxFailedLogins = 0,
            LockoutMinutes = 0
        });

        Assert.Equal(4, errors.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not base64 !!")]
    [InlineData("AAAA")] // valid base64, but only 3 bytes
    public void The_encryption_key_must_be_base64_of_exactly_32_bytes(string key)
        => Assert.NotEmpty(SecurityOptionsValidator.Validate(new SecurityOptions { TwoFactorEncryptionKey = key }));

    [Fact]
    public void Errors_never_repeat_a_secret()
    {
        var jwt = ValidJwt();
        jwt.SigningKey = "short-secret-value";
        var security = new SecurityOptions { TwoFactorEncryptionKey = "another-secret-value" };

        var text = string.Join(" ", JwtOptionsValidator.Validate(jwt).Concat(SecurityOptionsValidator.Validate(security)));

        Assert.DoesNotContain("short-secret-value", text);
        Assert.DoesNotContain("another-secret-value", text);
    }
}
