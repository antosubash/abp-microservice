using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace Tasky.Projects.HttpApi.Client.ConsoleTestApp;

public class ConsoleTestAppHostedService(IConfiguration configuration) : IHostedService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (
            var application = await AbpApplicationFactory
                .CreateAsync<ProjectsConsoleApiClientModule>(options =>
                {
                    options.Services.ReplaceConfiguration(_configuration);
                    options.UseAutofac();
                })
                .ConfigureAwait(false)
        )
        {
            await application.InitializeAsync().ConfigureAwait(false);

            var demo = application.ServiceProvider.GetRequiredService<ClientDemoService>();
            await demo.RunAsync().ConfigureAwait(false);

            await application.ShutdownAsync().ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
