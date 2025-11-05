using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Tasky.IntegrationTests.Shared;

public class GatewayTests : IAsyncLifetime
{
    private DistributedApplication? _app;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Tasky.AppHost.Program>(
                args: ["--skip-dcp"],
                configureBuilder: (appOptions, hostSettings) =>
                {
                    appOptions.DisableDashboard = true;
                });

        _app = await builder.BuildAsync();
        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task Gateway_OpenApiEndpoint_IsAccessibleAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/openapi/v1.json");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Gateway OpenAPI endpoint responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Gateway_ScalarApiReference_IsAccessibleAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/scalar/v1");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Gateway Scalar API Reference endpoint responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Gateway_ReverseProxy_IsConfiguredAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Gateway reverse proxy responded with status: {response.StatusCode}"
        );
    }
}

