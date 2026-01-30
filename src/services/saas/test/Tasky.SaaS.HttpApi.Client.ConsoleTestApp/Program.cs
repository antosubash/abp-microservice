using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tasky.SaaS.HttpApi.Client.ConsoleTestApp;

internal static class Program
{
    private static Task Main(string[] args)
    {
        return CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .ConfigureServices((hostContext, services) => services.AddHostedService<ConsoleTestAppHostedService>());
    }
}
