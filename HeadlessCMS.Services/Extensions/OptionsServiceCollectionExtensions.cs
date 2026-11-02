using HeadlessCMS.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HeadlessCMS.Services;

public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Binds the Jwt, Auth and Security settings, checks them all, and registers them as <see cref="IOptions{T}"/>.
    /// If anything is missing or invalid the app refuses to start, with one message listing every problem.
    /// </summary>
    public static IServiceCollection AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = Bind<JwtOptions>(configuration, JwtOptions.SectionName);
        var auth = Bind<AuthOptions>(configuration, AuthOptions.SectionName);
        var security = Bind<SecurityOptions>(configuration, SecurityOptions.SectionName);

        var problems = new List<string>();
        problems.AddRange(JwtOptionsValidator.Validate(jwt));
        problems.AddRange(AuthOptionsValidator.Validate(auth));
        problems.AddRange(SecurityOptionsValidator.Validate(security));

        if (problems.Count > 0)
        {
            throw new InvalidOperationException(
                "The app cannot start because some settings are missing or invalid:" + Environment.NewLine
                + string.Join(Environment.NewLine, problems.Select(p => " - " + p)) + Environment.NewLine
                + "Set them as environment variables (use __ for sections, for example Jwt__SigningKey), in .env, or in user-secrets. See the README.");
        }

        services.AddSingleton(Options.Create(jwt));
        services.AddSingleton(Options.Create(auth));
        services.AddSingleton(Options.Create(security));

        return services;
    }

    private static T Bind<T>(IConfiguration configuration, string section) where T : new()
        => configuration.GetSection(section).Get<T>() ?? new T();
}
