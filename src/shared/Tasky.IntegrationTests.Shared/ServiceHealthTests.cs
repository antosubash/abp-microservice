using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Tasky.IntegrationTests.Shared;

public class ServiceHealthTests : IAsyncLifetime
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
    public async Task AdministrationApi_HealthEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.AdministrationApi);

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdministrationApi_AliveEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.AdministrationApi);

        var response = await httpClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task IdentityServiceApi_HealthEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.IdentityServiceApi);

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task IdentityServiceApi_AliveEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.IdentityServiceApi);

        var response = await httpClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SaaSApi_HealthEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.SaaSApi);

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SaaSApi_AliveEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.SaaSApi);

        var response = await httpClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProjectsApi_HealthEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.ProjectsApi);

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProjectsApi_AliveEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.ProjectsApi);

        var response = await httpClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Gateway_HealthEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Gateway_AliveEndpoint_ReturnsHealthyAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

