namespace HeadlessCMS.Common.Options;

/// <summary>Settings for the JWT access tokens. Bound from the "Jwt" section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Secret used to sign tokens. At least 32 bytes. It has no default: set it as a secret.</summary>
    public string SigningKey { get; set; } = "";

    public string Issuer { get; set; } = "";

    public string Audience { get; set; } = "";

    /// <summary>How long an access token is valid. Default: 15 minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 15;
}
