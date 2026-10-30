using HeadlessCMS.API.Contracts.Auth;
using HeadlessCMS.Services.Auth;

namespace HeadlessCMS.API.Mapping;

/// <summary>
/// Manual mapping between the API's contracts and the Services layer's input and result types.
/// Every field that leaves the API is written out here on purpose.
/// The API never sees an entity: Services return result records, and those are mapped to responses.
/// </summary>
public static class AuthMappings
{
    public static RegisterInput ToInput(this RegisterRequest request)
        => new(request.Email, request.Password, request.Name);

    public static UserResponse ToResponse(this UserResult user)
        => new(user.Id, user.Email, user.Name);
}
