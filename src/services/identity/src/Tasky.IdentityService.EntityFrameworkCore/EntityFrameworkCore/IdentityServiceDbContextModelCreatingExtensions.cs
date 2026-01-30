using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Tasky.IdentityService.EntityFrameworkCore;

public static class IdentityServiceDbContextModelCreatingExtensions
{
    public static void ConfigureIdentityService(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
