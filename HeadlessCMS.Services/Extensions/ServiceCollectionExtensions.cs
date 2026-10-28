using HeadlessCMS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HeadlessCMS.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything the Services layer needs, including the Data layer beneath it.
    /// The API project calls only this, so it never has to know about Data.
    /// </summary>
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDataServices(configuration, environment);

        // Register the application's services here, for example:
        // services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>Runs start-up checks and migrations. Call once, after the app is built.</summary>
    public static Task InitServicesAsync(this IServiceProvider serviceProvider)
        => serviceProvider.InitDataServicesAsync();
}
