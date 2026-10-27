namespace HeadlessCMS.Common.Enums;

public enum EventType
{
    UserRegistered,
    LoginSucceeded,
    LoginFailed,
    PasswordChanged,
    TwoFactorEnabled,
    TwoFactorDisabled,
    ApiTokenCreated,
    ApiTokenRevoked
}
