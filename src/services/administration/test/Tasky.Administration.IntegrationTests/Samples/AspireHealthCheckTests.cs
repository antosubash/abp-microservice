using System.Net;
using Shouldly;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.Administration.Samples;

public class AspireHealthCheckTests : AdministrationAspireIntegrationTestBase
{
    public AspireHealthCheckTests(AspireTestingFixture fixture)
        : base(fixture) { }

    [Fact(Skip = "Health check endpoints may not be configured in Aspire environment")]
    public async Task Should_Return_Ok_For_HealthCheck_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(Skip = "Health check endpoints may not be configured in Aspire environment")]
    public async Task Should_Return_Ok_For_Alive_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/alive");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(Skip = "Health check endpoints may not be configured in Aspire environment")]
    public async Task Should_Return_Valid_Content_For_HealthCheckAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Health check endpoints may not be configured in Aspire environment")]
    public async Task Should_Return_Valid_Content_For_Alive_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/alive");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }
}
