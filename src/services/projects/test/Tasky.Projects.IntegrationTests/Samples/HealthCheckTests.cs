using System.Net;
using Shouldly;
using Tasky.Projects.Fixtures;
using Xunit;

namespace Tasky.Projects.Samples;

public class HealthCheckTests : ProjectsIntegrationTestBase
{
    public HealthCheckTests(TestcontainersFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Should_Return_Ok_For_HealthCheckAsync()
    {
        var response = await Client.GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return_Ok_For_Alive_EndpointAsync()
    {
        var response = await Client.GetAsync("/alive");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

