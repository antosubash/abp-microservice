using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Tasky.Administration.Fixtures;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.Administration.Samples;

public class ApiEndpointTests : AdministrationIntegrationTestBase
{
    public ApiEndpointTests(TestcontainersFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Should_Return_Ok_For_Sample_EndpointAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Authorized_Endpoint_Without_TokenAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample/authorized");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_Ok_For_Authorized_Endpoint_With_TokenAsync()
    {
        var tokenHeader = GetBearerTokenHeader(
            userName: "testuser",
            email: IntegrationTestConstants.TestAdminEmail
        );
        SetAuthorizationHeader(Client, tokenHeader);

        var response = await Client.GetAsync("/api/Administration/sample/authorized");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Json_Content_For_Sample_EndpointAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldContain("application/json");
    }
}

