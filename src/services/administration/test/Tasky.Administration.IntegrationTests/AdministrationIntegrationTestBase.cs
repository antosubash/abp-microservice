using Tasky.Administration.Fixtures;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.Administration;

[Collection("IntegrationTests")]
public abstract class AdministrationIntegrationTestBase : IDisposable
{
    protected AdministrationWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }
    protected TestcontainersFixture Fixture { get; }

    protected AdministrationIntegrationTestBase(TestcontainersFixture fixture)
    {
        Fixture = fixture;
        Factory = new AdministrationWebApplicationFactory(fixture);
        Client = Factory.CreateClient();
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

    public virtual void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}

