using HeadlessCMS.API.Contracts.Auth;
using HeadlessCMS.API.Mapping;
using HeadlessCMS.Services.Auth;

namespace HeadlessCMS.Tests.Unit.Contracts;

/// <summary>REFERENCE EXAMPLE: a mapping test checks every field, in both directions.</summary>
public class AuthMappingsTests
{
    [Fact]
    public void A_request_maps_to_the_service_input()
    {
        var input = new RegisterRequest("ada@example.com", "a-long-password-1", "Ada").ToInput();

        Assert.Equal("ada@example.com", input.Email);
        Assert.Equal("a-long-password-1", input.Password);
        Assert.Equal("Ada", input.Name);
    }

    [Fact]
    public void A_service_result_maps_to_a_response()
    {
        var id = Guid.NewGuid();

        var response = new UserResult(id, "ada@example.com", "Ada").ToResponse();

        Assert.Equal(id, response.Id);
        Assert.Equal("ada@example.com", response.Email);
        Assert.Equal("Ada", response.Name);
    }

    [Fact]
    public void Printing_a_request_or_input_never_shows_the_password()
    {
        var request = new RegisterRequest("ada@example.com", "super-secret-password", "Ada");

        Assert.DoesNotContain("super-secret-password", request.ToString());
        Assert.DoesNotContain("super-secret-password", request.ToInput().ToString());
    }
}
