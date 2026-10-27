using Logship.Options;

namespace HeadlessCMS.API.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Turns on Logship's file and Loki sinks from the "Logship" configuration section.
    /// A sink with no settings stays off, so local development needs no Loki.
    /// Keep passwords and tokens out of appsettings.json; set them as environment variables
    /// (for example Logship__Loki__Password) or in user-secrets.
    /// </summary>
    public static ILoggingBuilder AddLogshipFromConfiguration(
        this ILoggingBuilder logging,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var section = configuration.GetSection("Logship");

        return logging.AddLogship(options =>
        {
            var lokiUrl = section["Loki:Url"];
            if (!string.IsNullOrWhiteSpace(lokiUrl))
            {
                options.Loki = new LokiSinkOptions
                {
                    Url = lokiUrl,
                    AppLabel = section["Loki:AppLabel"] ?? "headless-cms",
                    JobLabel = section["Loki:JobLabel"] ?? "headless-cms",
                    Environment = environment.EnvironmentName.ToLowerInvariant(),
                    OrgId = section["Loki:OrgId"],
                    AuthType = Enum.TryParse<LokiAuthType>(section["Loki:AuthType"], ignoreCase: true, out var authType)
                        ? authType
                        : LokiAuthType.None,
                    Username = section["Loki:Username"],
                    Password = section["Loki:Password"],
                    Token = section["Loki:Token"]
                };
            }

            if (section.GetValue<bool>("File:Enabled"))
            {
                options.File = new FileSinkOptions
                {
                    Directory = section["File:Directory"] ?? "logs"
                };
            }
        });
    }
}
