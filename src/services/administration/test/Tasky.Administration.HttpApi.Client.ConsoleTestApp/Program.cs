using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tasky.Administration.HttpApi.Client.ConsoleTestApp;

internal class Program
{
    private static Task Main(string[] args)
    {
        return CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddHostedService<ConsoleTestAppHostedService>();
                }
            );
    }
}
