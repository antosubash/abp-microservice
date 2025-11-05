using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Tasky.IntegrationTests.Shared;

public class AppHostTests : IAsyncLifetime
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
    public async Task AdministrationApi_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.AdministrationApi);

        var response = await httpClient.GetAsync("/swagger");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Service responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task IdentityServiceApi_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.IdentityServiceApi);

        var response = await httpClient.GetAsync("/swagger");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Service responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task SaaSApi_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.SaaSApi);

        var response = await httpClient.GetAsync("/swagger");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Service responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task ProjectsApi_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.ProjectsApi);

        var response = await httpClient.GetAsync("/swagger");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Service responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Gateway_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.Gateway);

        var response = await httpClient.GetAsync("/");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"Gateway responded with status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AuthServer_RespondsToRequestsAsync()
    {
        Assert.NotNull(_app);
        var httpClient = _app!.CreateHttpClient(TaskyNames.AuthServer);

        var response = await httpClient.GetAsync("/");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently,
            $"AuthServer responded with status: {response.StatusCode}"
        );
    }
}

