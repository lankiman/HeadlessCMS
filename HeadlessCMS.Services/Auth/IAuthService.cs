namespace HeadlessCMS.Services.Auth;

/// <summary>What the API can ask of the auth part of the application.</summary>
public interface IAuthService
{
    /// <summary>Creates a user with a personal workspace and an owner membership, in one transaction.</summary>
    Task<UserResult> RegisterAsync(RegisterInput input, CancellationToken cancellationToken = default);
}
