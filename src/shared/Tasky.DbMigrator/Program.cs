using Serilog;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.Projects.EntityFrameworkCore;
using Tasky.SaaS.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace Tasky.DbMigrator;

internal static class Program
{
    private static Task Main(string[] args)
    {
        TaskyLogging.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<AdministrationDbContext>(connectionName: TaskyNames.AdministrationDb);
        builder.AddNpgsqlDbContext<IdentityDbContext>(connectionName: TaskyNames.IdentityServiceDb);
        builder.AddNpgsqlDbContext<SaaSDbContext>(connectionName: TaskyNames.SaaSDb);
        builder.AddNpgsqlDbContext<ProjectsDbContext>(connectionName: TaskyNames.ProjectsDb);

        builder.Configuration.AddAppSettingsSecretsJson();

        builder.Logging.AddSerilog();

        builder.Services.AddHostedService<DbMigratorHostedService>();

        var host = builder.Build();

        return host.RunAsync();
    }
}
