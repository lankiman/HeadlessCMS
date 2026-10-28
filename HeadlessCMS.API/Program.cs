using HeadlessCMS.API.Extensions;
using HeadlessCMS.Services;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLogshipFromConfiguration(builder.Configuration, builder.Environment);

builder.Services.AddApiServices();
builder.Services.AddServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseHttpsRedirection();

// Checks Redis and applies EF Core migrations
await app.Services.InitServicesAsync();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "HeadlessCMS API");
    options.RoutePrefix = "documentation";
});

app.MapGet("/", () => "Hello World");

app.Run();
