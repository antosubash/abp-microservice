namespace Tasky.DbMigrator.Model;

public class ServiceClient
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string[] RootUrls { get; set; } = [];

    public string[] Scopes { get; set; } = [];

    public string[] GrantTypes { get; set; } = [];

    public string[] RedirectUris { get; set; } = [];

    public string[] PostLogoutRedirectUris { get; set; } = [];

    public string[] AllowedCorsOrigins { get; set; } = [];
}
