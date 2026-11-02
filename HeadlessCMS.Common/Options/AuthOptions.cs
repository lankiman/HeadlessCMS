namespace HeadlessCMS.Common.Options;

/// <summary>Settings for sessions and login protection. Bound from the "Auth" section.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>How long a refresh token is valid. Default: 30 days.</summary>
    public int RefreshTokenDays { get; set; } = 30;

    /// <summary>How long the step between the password and the 2FA code lasts. Default: 5 minutes.</summary>
    public int PendingTwoFactorMinutes { get; set; } = 5;

    /// <summary>Failed logins allowed before an account is locked. Default: 5.</summary>
    public int MaxFailedLogins { get; set; } = 5;

    /// <summary>How long a lock lasts. Default: 15 minutes.</summary>
    public int LockoutMinutes { get; set; } = 15;
}
