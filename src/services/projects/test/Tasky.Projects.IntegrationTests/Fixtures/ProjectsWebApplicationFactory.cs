using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Tasky.Projects.Fixtures;

namespace Tasky.Projects.Fixtures;

public class ProjectsWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestcontainersFixture _fixture;

    public ProjectsWebApplicationFactory(TestcontainersFixture fixture)
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
                    ["ConnectionStrings:TaskyProjectsDb"] = _fixture.ProjectsConnectionString
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

