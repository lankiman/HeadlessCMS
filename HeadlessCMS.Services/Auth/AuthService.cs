namespace HeadlessCMS.Services.Auth;

/// <summary>The auth logic. The security-critical parts are written by hand by the maintainer.</summary>
public class AuthService : IAuthService
{
    public Task<UserResult> RegisterAsync(RegisterInput input, CancellationToken cancellationToken = default)
    {
        // HUMAN: create the user, their personal workspace and the owner membership in one transaction.
        throw new NotImplementedException();
    }
}
