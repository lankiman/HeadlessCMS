using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace HeadlessCMS.API.Extensions;

public static class ValidationExtensions
{
    /// <summary>Turns FluentValidation's result into the standard 400 body: property name → its error messages.</summary>
    public static ValidationProblemDetails ToProblemDetails(this ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors);
    }
}
