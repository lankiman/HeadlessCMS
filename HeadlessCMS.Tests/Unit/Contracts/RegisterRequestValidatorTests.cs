using HeadlessCMS.API.Contracts.Auth;

namespace HeadlessCMS.Tests.Unit.Contracts;

/// <summary>REFERENCE EXAMPLE: one test per validation rule, written the same way for every validator.</summary>
public class RegisterRequestValidatorTests
{
    private static readonly RegisterRequestValidator Validator = new();

    private static RegisterRequest Valid() => new("ada@example.com", "a-long-password-1", "Ada");

    private static string[] FailedProperties(RegisterRequest request)
        => Validator.Validate(request).Errors.Select(e => e.PropertyName).Distinct().ToArray();

    [Fact]
    public void A_valid_request_passes()
        => Assert.True(Validator.Validate(Valid()).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void A_bad_email_is_rejected(string email)
        => Assert.Contains(nameof(RegisterRequest.Email), FailedProperties(Valid() with { Email = email }));

    [Fact]
    public void An_email_over_255_characters_is_rejected()
    {
        var email = new string('a', 250) + "@example.com";

        Assert.Contains(nameof(RegisterRequest.Email), FailedProperties(Valid() with { Email = email }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("elevenchars")] // 11 characters: one short of the minimum
    public void A_short_or_empty_password_is_rejected(string password)
        => Assert.Contains(nameof(RegisterRequest.Password), FailedProperties(Valid() with { Password = password }));

    [Fact]
    public void Passwords_of_12_and_128_characters_are_accepted()
    {
        Assert.True(Validator.Validate(Valid() with { Password = new string('a', 12) }).IsValid);
        Assert.True(Validator.Validate(Valid() with { Password = new string('a', 128) }).IsValid);
    }

    [Fact]
    public void A_password_over_128_characters_is_rejected()
        => Assert.Contains(nameof(RegisterRequest.Password), FailedProperties(Valid() with { Password = new string('a', 129) }));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void An_empty_name_is_rejected(string name)
        => Assert.Contains(nameof(RegisterRequest.Name), FailedProperties(Valid() with { Name = name }));

    [Fact]
    public void A_name_over_100_characters_is_rejected()
        => Assert.Contains(nameof(RegisterRequest.Name), FailedProperties(Valid() with { Name = new string('a', 101) }));

    [Fact]
    public void Every_problem_is_reported_together()
    {
        var failed = FailedProperties(new RegisterRequest("", "", ""));

        Assert.Equal(3, failed.Length);
    }
}
