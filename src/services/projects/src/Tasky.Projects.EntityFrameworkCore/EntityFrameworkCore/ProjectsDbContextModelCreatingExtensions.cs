using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Tasky.Projects.EntityFrameworkCore;

public static class ProjectsDbContextModelCreatingExtensions
{
    public static void ConfigureProjects(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
