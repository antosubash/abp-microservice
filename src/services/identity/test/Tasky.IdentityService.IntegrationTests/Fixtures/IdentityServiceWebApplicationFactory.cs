using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Tasky.IdentityService.Fixtures;

namespace Tasky.IdentityService.Fixtures;

public class IdentityServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestcontainersFixture _fixture;

    public IdentityServiceWebApplicationFactory(TestcontainersFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TaskyAdministrationDb"] =
                        _fixture.AdministrationConnectionString,
                    ["ConnectionStrings:TaskyIdentityServiceDb"] =
                        _fixture.IdentityConnectionString,
                    ["ConnectionStrings:TaskySaaSDb"] = _fixture.SaaSConnectionString
                }
            );
        });

        builder.UseEnvironment("Test");
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.Timeout = TimeSpan.FromMinutes(5);
    }
}

