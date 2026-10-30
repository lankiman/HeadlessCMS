namespace HeadlessCMS.Services.Auth;

/// <summary>
/// What the Services layer needs to register a user. The API maps its request into this,
/// so Services never depends on the API's contracts.
/// </summary>
public record RegisterInput(string Email, string Password, string Name)
{
    // A record prints all its fields. Keep the password out of logs and error messages.
    public override string ToString() => $"{nameof(RegisterInput)} {{ Email = {Email}, Name = {Name} }}";
}
