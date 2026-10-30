namespace HeadlessCMS.Services.Auth;

/// <summary>
/// What the Services layer returns about a user. Entities such as <c>User</c> never leave Services,
/// because the API must not depend on the Data layer. Only fields that are safe to show are included.
/// </summary>
public record UserResult(Guid Id, string Email, string Name);
