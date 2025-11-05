using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Tasky.IntegrationTests.Shared;

public class InfrastructureTests : IAsyncLifetime
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
    public async Task Services_CanConnectToDatabasesAsync()
    {
        Assert.NotNull(_app);

        var adminClient = _app!.CreateHttpClient(TaskyNames.AdministrationApi);
        var response = await adminClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Services_CanConnectToRedisAsync()
    {
        Assert.NotNull(_app);

        var adminClient = _app!.CreateHttpClient(TaskyNames.AdministrationApi);
        var response = await adminClient.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Services_CanConnectToRabbitMQAsync()
    {
        Assert.NotNull(_app);

        var identityClient = _app!.CreateHttpClient(TaskyNames.IdentityServiceApi);
        var response = await identityClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Services_CanConnectToSeqAsync()
    {
        Assert.NotNull(_app);

        var projectsClient = _app!.CreateHttpClient(TaskyNames.ProjectsApi);
        var response = await projectsClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
