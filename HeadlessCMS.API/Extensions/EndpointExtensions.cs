using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace HeadlessCMS.API.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// /health says the process is alive and runs no checks.
    /// /health/ready checks PostgreSQL, MongoDB and Redis and answers 503 if one is down.
    /// Both reply with a single word, never with connection details.
    /// </summary>
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

        return app;
    }
}
