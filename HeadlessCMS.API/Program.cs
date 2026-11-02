using HeadlessCMS.API.Extensions;
using HeadlessCMS.Services;

// Real environment variables win over the .env file. The file is searched for in this folder and every folder above it.
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLogshipFromConfiguration(builder.Configuration, builder.Environment);

builder.Services.AddApiServices();
builder.Services.AddServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// "dotnet HeadlessCMS.API.dll --migrate" applies the database migrations and exits. Use it as a deploy step in production.
if (args.Contains("--migrate"))
{
    await app.Services.MigrateServicesAsync();
    return;
}

app.UseHttpsRedirection();

// Checks Redis and MongoDB, and applies migrations when that is switched on
await app.Services.InitServicesAsync();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "HeadlessCMS API");
    options.RoutePrefix = "documentation";
});

app.MapHealthEndpoints();
app.MapControllers();

app.MapGet("/", () => "Hello World");

app.Run();

// Lets the tests start the whole app with WebApplicationFactory<Program>.
public partial class Program;
