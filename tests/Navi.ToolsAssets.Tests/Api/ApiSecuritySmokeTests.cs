using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Navi.ToolsAssets.Tests.Api;

public sealed class ApiSecuritySmokeTests : IClassFixture<NaviApiFactory>
{
    private readonly HttpClient _client;

    public ApiSecuritySmokeTests(NaviApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Root_IsAnonymousAndEmitsSecurityHeaders()
    {
        using var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("Permissions-Policy"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsAnonymousAndForgedIdentityHeaders()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tools");
        request.Headers.Add("X-Navi-User", "admin");
        request.Headers.Add("X-Navi-Role", "ADMIN");
        request.Headers.Add("X-Navi-Permission", "*");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LiveHealth_IsAnonymous()
    {
        using var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_IsUnavailableOutsideDevelopment()
    {
        using var response = await _client.GetAsync("/swagger/index.html");

        Assert.Contains(
            response.StatusCode,
            new[] { HttpStatusCode.NotFound, HttpStatusCode.Unauthorized });
    }
}

public sealed class NaviApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:NaviToolsAssetsDb"] =
                    "Server=127.0.0.1,1;Database=NaviTesting;User Id=test;Password=NotUsed_2026!;TrustServerCertificate=True;Connect Timeout=1",
                ["ConnectionStrings:NaviMasterDb"] = string.Empty,
                ["Jwt:SigningKey"] =
                    "NAVI_TEST_ONLY_SIGNING_KEY_2026_ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789",
                ["Jwt:Issuer"] = "Navi.ToolsAssets.Api",
                ["Jwt:Audience"] = "Navi.ToolsAssets.Clients",
                ["Tenancy:Enabled"] = "false",
                ["Minio:AccessKey"] = string.Empty,
                ["Minio:SecretKey"] = string.Empty
            });
        });
    }
}
