namespace HeadlessCMS.Common.Options;

/// <summary>Keys the app uses to protect data it stores. Bound from the "Security" section.</summary>
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>
    /// Key that encrypts 2FA secrets in the database: a base64 string that decodes to exactly 32 bytes.
    /// It has no default: set it as a secret. Losing it makes every enrolled 2FA secret unreadable.
    /// </summary>
    public string TwoFactorEncryptionKey { get; set; } = "";
}
