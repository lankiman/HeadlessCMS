namespace HeadlessCMS.API.Contracts.Auth;

/// <summary>
/// What a client gets back about a user. List only fields that are safe to show:
/// never a password hash, a token hash or a 2FA secret. A test fails if a response type has one.
/// </summary>
public record UserResponse(Guid Id, string Email, string Name);
