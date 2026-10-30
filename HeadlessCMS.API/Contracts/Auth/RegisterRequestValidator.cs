using FluentValidation;

namespace HeadlessCMS.API.Contracts.Auth;

/// <summary>
/// Checks the shape of a <see cref="RegisterRequest"/>. Rules that need the database, such as
/// "this email is already taken", belong in the service. The unique index is the final safeguard.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(255).EmailAddress();

        // The upper limit stops someone sending a huge string to make password hashing slow.
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(128);

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
