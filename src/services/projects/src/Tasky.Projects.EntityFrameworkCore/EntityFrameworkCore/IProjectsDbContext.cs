using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.Projects.EntityFrameworkCore;

[ConnectionStringName(TaskyNames.ProjectsDb)]
public interface IProjectsDbContext : IEfCoreDbContext { }
