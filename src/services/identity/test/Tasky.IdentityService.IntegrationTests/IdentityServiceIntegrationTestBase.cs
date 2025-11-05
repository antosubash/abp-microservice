using Tasky.IdentityService.Fixtures;
using Tasky.IntegrationTests.Shared;
using Xunit;

namespace Tasky.IdentityService;

[Collection("IntegrationTests")]
public abstract class IdentityServiceIntegrationTestBase : IDisposable
{
    protected IdentityServiceWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }
    protected TestcontainersFixture Fixture { get; }

    protected IdentityServiceIntegrationTestBase(TestcontainersFixture fixture)
    {
        Fixture = fixture;
        Factory = new IdentityServiceWebApplicationFactory(fixture);
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

