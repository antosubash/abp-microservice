using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Tasky.IntegrationTests.Shared;

public static class AuthenticationTestHelper
{
    public static string CreateTestJwtToken(
        string? userName = null,
        string? email = null,
        Guid? tenantId = null,
        List<string>? roles = null
    )
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(userName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userName));
        }

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenantid", tenantId.Value.ToString()));
        }

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("IntegrationTestSecretKeyThatMustBeAtLeast32CharactersLong")
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: IntegrationTestConstants.TestAuthServerUrl,
            audience: IntegrationTestConstants.TestAuthServerUrl,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GetBearerTokenHeader(string token)
    {
        return $"Bearer {token}";
    }
}

