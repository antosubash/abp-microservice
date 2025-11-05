using System.Net;
using Shouldly;
using Tasky.SaaS.Fixtures;
using Xunit;

namespace Tasky.SaaS.Samples;

public class HealthCheckTests : SaaSIntegrationTestBase
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

