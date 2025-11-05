using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tasky.IntegrationTests.Shared;

public sealed class AspireTestingFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost.Program>();

        builder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler();
        });

        _app = await builder.BuildAsync();
        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync().ConfigureAwait(false);
    }

    public async Task<HttpClient> CreateHttpClientAsync(string applicationName)
    {
        if (_app is null)
            throw new InvalidOperationException("DistributedApplication is null. Ensure InitializeAsync has been called.");

        var client = _app.CreateHttpClient(applicationName);

        // Wait for the resource to be running
        // The CreateHttpClient method should handle waiting, but we add a small delay to ensure readiness
        await Task.Delay(TimeSpan.FromSeconds(2));

        return client;
    }

    public DistributedApplication GetApplication()
    {
        if (_app is null)
            throw new InvalidOperationException("DistributedApplication is null. Ensure InitializeAsync has been called.");

        return _app;
    }
}
