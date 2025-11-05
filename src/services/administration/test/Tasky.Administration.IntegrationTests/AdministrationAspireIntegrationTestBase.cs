using Microsoft.Extensions.Hosting;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.Administration;

[Collection("AspireIntegrationTests")]
public abstract class AdministrationAspireIntegrationTestBase : IClassFixture<AspireTestingFixture>
{
    protected AspireTestingFixture Fixture { get; }
    protected HttpClient Client { get; private set; } = null!;

    protected AdministrationAspireIntegrationTestBase(AspireTestingFixture fixture)
    {
        Fixture = fixture;
    }

    protected async Task<HttpClient> GetClientAsync()
    {
        if (Client is null)
        {
            Client = await Fixture.CreateHttpClientAsync(TaskyNames.AdministrationApi);
        }

        return Client;
    }

    protected string GetBearerTokenHeader(
        string? userName = null,
        string? email = null,
        Guid? tenantId = null,
        List<string>? roles = null
    )
    {
        var token = AuthenticationTestHelper.CreateTestJwtToken(userName, email, tenantId, roles);
        return AuthenticationTestHelper.GetBearerTokenHeader(token);
    }

    protected void SetAuthorizationHeader(HttpClient client, string bearerTokenHeader)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", bearerTokenHeader);
    }
}
