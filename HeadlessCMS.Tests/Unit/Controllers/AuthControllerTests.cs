using HeadlessCMS.API.Contracts.Auth;
using HeadlessCMS.API.Controllers;
using HeadlessCMS.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HeadlessCMS.Tests.Unit.Controllers;

/// <summary>
/// REFERENCE EXAMPLE: a controller is tested on its own with a hand-written fake of the service,
/// so the test checks only what the controller does: validate, map, call, map back.
/// </summary>
public class AuthControllerTests
{
    private sealed class FakeAuthService : IAuthService
    {
        public RegisterInput? Received { get; private set; }

        public Task<UserResult> RegisterAsync(RegisterInput input, CancellationToken cancellationToken = default)
        {
            Received = input;
            return Task.FromResult(new UserResult(Guid.Parse("11111111-1111-1111-1111-111111111111"), input.Email, input.Name));
        }
    }

    private static AuthController CreateController(FakeAuthService service)
        => new(service, new RegisterRequestValidator());

    [Fact]
    public async Task A_valid_request_creates_the_user_and_returns_201_with_a_safe_response()
    {
        var service = new FakeAuthService();

        var result = await CreateController(service).Register(
            new RegisterRequest("ada@example.com", "a-long-password-1", "Ada"), CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<UserResponse>(created.Value);
        Assert.Equal("ada@example.com", response.Email);
        Assert.Equal("Ada", response.Name);

        // The service received the mapped input.
        Assert.Equal("a-long-password-1", service.Received!.Password);
    }

    [Fact]
    public async Task An_invalid_request_returns_400_and_never_reaches_the_service()
    {
        var service = new FakeAuthService();

        var result = await CreateController(service).Register(
            new RegisterRequest("not-an-email", "short", ""), CancellationToken.None);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.Equal(400, problem.StatusCode);
        Assert.Contains(nameof(RegisterRequest.Email), details.Errors.Keys);
        Assert.Contains(nameof(RegisterRequest.Password), details.Errors.Keys);
        Assert.Contains(nameof(RegisterRequest.Name), details.Errors.Keys);
        Assert.Null(service.Received);
    }
}
