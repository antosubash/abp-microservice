using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Tasky.SaaS.EntityFrameworkCore;

public static class SaaSDbContextModelCreatingExtensions
{
    public static void ConfigureSaaS(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
