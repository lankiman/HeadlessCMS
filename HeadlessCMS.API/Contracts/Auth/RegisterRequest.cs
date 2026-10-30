namespace HeadlessCMS.API.Contracts.Auth;

/// <summary>What a client sends to create an account.</summary>
/// <remarks>
/// REFERENCE EXAMPLE: copy this pattern for every contract.
/// A request is a record in <c>Contracts/&lt;Feature&gt;/</c>, with its validator beside it.
/// </remarks>
public record RegisterRequest(string Email, string Password, string Name)
{
    // A record prints all its fields. Keep the password out of logs and error messages.
    public override string ToString() => $"{nameof(RegisterRequest)} {{ Email = {Email}, Name = {Name} }}";
}
