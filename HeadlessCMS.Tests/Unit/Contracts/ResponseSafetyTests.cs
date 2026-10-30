using System.Text.RegularExpressions;
using HeadlessCMS.API.Contracts.Auth;

namespace HeadlessCMS.Tests.Unit.Contracts;

/// <summary>
/// A safety net for every response type, including ones an AI agent writes later:
/// a response must never expose a password, a hash or a secret.
/// (Tokens are allowed: a login response legitimately returns one.)
/// </summary>
public class ResponseSafetyTests
{
    private static readonly Regex Forbidden = new("password|hash|secret", RegexOptions.IgnoreCase);

    [Fact]
    public void No_response_type_exposes_a_password_hash_or_secret()
    {
        var responseTypes = typeof(UserResponse).Assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("HeadlessCMS.API.Contracts", StringComparison.Ordinal) == true)
            .Where(t => t.Name.EndsWith("Response", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(responseTypes);

        foreach (var type in responseTypes)
        {
            var unsafeProperties = type.GetProperties()
                .Select(p => p.Name)
                .Where(name => Forbidden.IsMatch(name))
                .ToList();

            Assert.True(unsafeProperties.Count == 0, $"{type.Name} exposes: {string.Join(", ", unsafeProperties)}");
        }
    }
}
