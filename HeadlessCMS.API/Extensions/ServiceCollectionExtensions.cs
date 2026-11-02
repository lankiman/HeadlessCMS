using FluentValidation;
using HeadlessCMS.API.Contracts.Auth;
using Microsoft.OpenApi;

namespace HeadlessCMS.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the API's own services. Everything below the API is registered by AddServices.</summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Use a relative server URL so Swagger UI calls the origin it was loaded from (HTTPS on Railway),
                // not an internal address such as http://localhost:8080.
                document.Servers = [new OpenApiServer { Url = "/" }];

                return Task.CompletedTask;
            });
        });

        // The fixed API (auth, workspaces, tokens) uses controllers. The generated record routes
        // (/api/{schemaNamePlural}) will use endpoint routing instead, in the Endpoints folder.
        services.AddControllers();

        // Finds every validator in this project, so a new validator needs no registration.
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
