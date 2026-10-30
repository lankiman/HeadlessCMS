using FluentValidation.Results;
using HeadlessCMS.API.Extensions;

namespace HeadlessCMS.Tests.Unit.Controllers;

public class ValidationExtensionsTests
{
    [Fact]
    public void Errors_are_grouped_by_property()
    {
        var result = new ValidationResult(
        [
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Email", "Email is not valid."),
            new ValidationFailure("Name", "Name is required.")
        ]);

        var details = result.ToProblemDetails();

        Assert.Equal(400, details.Status);
        Assert.Equal(2, details.Errors["Email"].Length);
        Assert.Equal(new[] { "Name is required." }, details.Errors["Name"]);
    }
}
