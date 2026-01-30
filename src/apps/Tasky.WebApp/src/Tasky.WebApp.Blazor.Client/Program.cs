using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Tasky.WebApp.Blazor.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var application = await builder
            .AddApplicationAsync<WebAppBlazorClientModule>(options => options.UseAutofac())
            .ConfigureAwait(false);

        var host = builder.Build();

        await application.InitializeApplicationAsync(host.Services).ConfigureAwait(false);

        await host.RunAsync().ConfigureAwait(false);
    }
}
