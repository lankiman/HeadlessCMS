using System.Net;
using System.Net.Http.Json;

namespace HeadlessCMS.Tests.Integration;

/// <summary>Real HTTP requests against the whole running API.</summary>
public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_says_the_process_is_alive()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_answers_503_when_the_databases_are_down_and_reveals_nothing()
    {
        var response = await _client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", body);
    }

    [Fact]
    public async Task The_OpenAPI_document_lists_the_controller_routes()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/auth/register", body);
    }

    [Fact]
    public async Task Swagger_UI_is_served()
    {
        var response = await _client.GetAsync("/documentation/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Registering_with_invalid_input_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new { email = "nope", password = "x", name = "" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email", body);
        Assert.Contains("Password", body);
    }

    [Fact]
    public async Task Unknown_routes_return_404()
    {
        var response = await _client.GetAsync("/no-such-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
