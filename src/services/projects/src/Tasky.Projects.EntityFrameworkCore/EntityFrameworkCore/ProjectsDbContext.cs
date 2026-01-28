using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.Projects.EntityFrameworkCore;

[ConnectionStringName(TaskyNames.ProjectsDb)]
public class ProjectsDbContext(DbContextOptions<ProjectsDbContext> options)
    : AbpDbContext<ProjectsDbContext>(options),
        IProjectsDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureProjects();
    }
}
