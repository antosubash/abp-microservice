using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Tasky.Administration.EntityFrameworkCore;

public static class AdministrationDbContextModelCreatingExtensions
{
    public static void ConfigureAdministration(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
