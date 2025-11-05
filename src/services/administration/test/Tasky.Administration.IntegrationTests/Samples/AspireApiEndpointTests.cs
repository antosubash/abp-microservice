using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Tasky.Administration.Samples;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.Administration.Samples;

public class AspireApiEndpointTests : AdministrationAspireIntegrationTestBase
{
    public AspireApiEndpointTests(AspireTestingFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Should_Return_Ok_For_Sample_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/api/Administration/sample");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Valid_SampleDto_From_Sample_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/api/Administration/sample");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.EnsureSuccessStatusCode();

        var sampleDto = await response.Content.ReadFromJsonAsync<SampleDto>();
        sampleDto.ShouldNotBeNull();
        sampleDto.Value.ShouldBe(42);
    }

    [Fact]
    public async Task Should_Return_Json_Content_Type_For_Sample_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/api/Administration/sample");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldContain("application/json");
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Authorized_Endpoint_Without_TokenAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/api/Administration/sample/authorized");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Authentication tests require real tokens from auth server in Aspire environment")]
    public async Task Should_Return_Ok_For_Authorized_Endpoint_With_TokenAsync()
    {
        var client = await GetClientAsync();
        var tokenHeader = GetBearerTokenHeader(
            userName: "testuser",
            email: IntegrationTestConstants.TestAdminEmail
        );
        SetAuthorizationHeader(client, tokenHeader);

        var response = await client.GetAsync("/api/Administration/sample/authorized");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Authentication tests require real tokens from auth server in Aspire environment")]
    public async Task Should_Return_Valid_SampleDto_From_Authorized_Endpoint_With_TokenAsync()
    {
        var client = await GetClientAsync();
        var tokenHeader = GetBearerTokenHeader(
            userName: "testuser",
            email: IntegrationTestConstants.TestAdminEmail
        );
        SetAuthorizationHeader(client, tokenHeader);

        var response = await client.GetAsync("/api/Administration/sample/authorized");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.EnsureSuccessStatusCode();

        var sampleDto = await response.Content.ReadFromJsonAsync<SampleDto>();
        sampleDto.ShouldNotBeNull();
        sampleDto.Value.ShouldBe(42);
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Authorized_Endpoint_With_Invalid_TokenAsync()
    {
        var client = await GetClientAsync();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await client.GetAsync("/api/Administration/sample/authorized");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_NotFound_For_NonExistent_EndpointAsync()
    {
        var client = await GetClientAsync();
        var response = await client.GetAsync("/api/Administration/nonexistent");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Requests_To_Same_EndpointAsync()
    {
        var client = await GetClientAsync();

        for (var i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/api/Administration/sample");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.EnsureSuccessStatusCode();

            var sampleDto = await response.Content.ReadFromJsonAsync<SampleDto>();
            sampleDto.ShouldNotBeNull();
            sampleDto.Value.ShouldBe(42);
        }
    }
}
