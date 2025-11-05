using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasky.Administration.Fixtures;

namespace Tasky.Administration.Fixtures;

public class AdministrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestcontainersFixture _fixture;

    public AdministrationWebApplicationFactory(TestcontainersFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var assemblyLocation = typeof(AdministrationWebApplicationFactory).Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;
            var testConfigPath = Path.Combine(assemblyDirectory, "appsettings.Test.json");

            if (File.Exists(testConfigPath))
            {
                config.AddJsonFile(testConfigPath, optional: false);
            }

            var isTestEnvironment = true;
            var configDict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:TaskyAdministrationDb"] =
                    _fixture.AdministrationConnectionString,
                ["ConnectionStrings:TaskyIdentityServiceDb"] =
                    _fixture.IdentityConnectionString
            };

            // Provide minimal valid connection strings for test environment to prevent null reference errors
            // These will fail to connect but allow modules to initialize without throwing null exceptions
            if (isTestEnvironment)
            {
                configDict["ConnectionStrings:redis"] = "localhost:6379,connectTimeout=1,abortConnect=false,syncTimeout=1";
                configDict["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@127.0.0.1:5672/";
                configDict["ConnectionStrings:seq"] = "http://127.0.0.1:5341";
            }

            config.AddInMemoryCollection(configDict);
        });

        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_fixture);

            // Configure in-memory distributed cache for tests to bypass Redis/Aspire requirements
            services.AddDistributedMemoryCache();
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.Timeout = TimeSpan.FromMinutes(5);
    }
}

