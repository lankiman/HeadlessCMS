using FluentValidation;
using HeadlessCMS.API.Contracts.Auth;
using HeadlessCMS.API.Extensions;
using HeadlessCMS.API.Mapping;
using HeadlessCMS.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HeadlessCMS.API.Controllers;

/// <remarks>
/// REFERENCE EXAMPLE: copy this pattern for every controller. A controller is thin. Each action does four things:
/// validate the request, map it to the service's input, call the service interface, and map the result to a response.
/// No business logic belongs here.
/// </remarks>
[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, IValidator<RegisterRequest> registerValidator) : ControllerBase
{
    /// <summary>Creates an account, together with a personal workspace.</summary>
    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation.ToProblemDetails());
        }

        var user = await authService.RegisterAsync(request.ToInput(), cancellationToken);

        return Created("/auth/me", user.ToResponse());
    }
}
