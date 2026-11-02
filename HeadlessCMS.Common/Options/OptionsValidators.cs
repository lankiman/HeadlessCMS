using System.Text;

namespace HeadlessCMS.Common.Options;

// Each validator returns every problem it finds. A message names the setting and never repeats its value,
// because the value may be a secret.

public static class JwtOptionsValidator
{
    public static List<string> Validate(JwtOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SigningKey))
            errors.Add("Jwt:SigningKey is required. Set it to a random string of at least 32 characters (environment variable Jwt__SigningKey).");
        else if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
            errors.Add("Jwt:SigningKey is too short. It must be at least 32 bytes.");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add("Jwt:Issuer is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add("Jwt:Audience is required.");

        if (options.AccessTokenMinutes is < 1 or > 1440)
            errors.Add("Jwt:AccessTokenMinutes must be between 1 and 1440.");

        return errors;
    }
}

public static class AuthOptionsValidator
{
    public static List<string> Validate(AuthOptions options)
    {
        var errors = new List<string>();

        if (options.RefreshTokenDays is < 1 or > 365)
            errors.Add("Auth:RefreshTokenDays must be between 1 and 365.");

        if (options.PendingTwoFactorMinutes is < 1 or > 60)
            errors.Add("Auth:PendingTwoFactorMinutes must be between 1 and 60.");

        if (options.MaxFailedLogins is < 1 or > 100)
            errors.Add("Auth:MaxFailedLogins must be between 1 and 100.");

        if (options.LockoutMinutes is < 1 or > 1440)
            errors.Add("Auth:LockoutMinutes must be between 1 and 1440.");

        return errors;
    }
}

public static class SecurityOptionsValidator
{
    private const int KeyBytes = 32;

    public static List<string> Validate(SecurityOptions options)
    {
        var errors = new List<string>();
        var key = options.TwoFactorEncryptionKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            errors.Add("Security:TwoFactorEncryptionKey is required (environment variable Security__TwoFactorEncryptionKey). Generate one with: openssl rand -base64 32");
        }
        else
        {
            var buffer = new byte[key.Length];
            if (!Convert.TryFromBase64String(key, buffer, out var written) || written != KeyBytes)
                errors.Add("Security:TwoFactorEncryptionKey must be a base64 string that decodes to exactly 32 bytes. Generate one with: openssl rand -base64 32");
        }

        return errors;
    }
}
