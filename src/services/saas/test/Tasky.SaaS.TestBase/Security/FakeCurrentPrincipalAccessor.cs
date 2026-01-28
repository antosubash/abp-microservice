using System.Security.Claims;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace Tasky.SaaS.Security;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    private readonly object _syncLock = new();
    private ClaimsPrincipal _principal;

    protected override ClaimsPrincipal GetClaimsPrincipal()
    {
        return GetPrincipal();
    }

    private ClaimsPrincipal GetPrincipal()
    {
        if (_principal == null)
        {
            lock (_syncLock)
            {
                if (_principal == null)
                {
                    _principal ??= new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
                                new(AbpClaimTypes.UserName, "admin"),
                                new(AbpClaimTypes.Email, "admin@abp.io"),
                            ]
                        )
                    );
                }
            }
        }

        return _principal;
    }
}
