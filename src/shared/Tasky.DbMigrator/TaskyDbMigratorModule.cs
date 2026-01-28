using Tasky.Administration;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.IdentityService;
using Tasky.IdentityService.EntityFrameworkCore;
using Tasky.Projects;
using Tasky.Projects.EntityFrameworkCore;
using Tasky.SaaS;
using Tasky.SaaS.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace Tasky.DbMigrator;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))]
[DependsOn(typeof(AdministrationEntityFrameworkCoreModule))]
[DependsOn(typeof(AdministrationApplicationContractsModule))]
[DependsOn(typeof(IdentityServiceEntityFrameworkCoreModule))]
[DependsOn(typeof(IdentityServiceApplicationContractsModule))]
[DependsOn(typeof(ProjectsEntityFrameworkCoreModule))]
[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(SaaSEntityFrameworkCoreModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
//[DependsOn(typeof(WebAppEntityFrameworkCoreModule))]
//[DependsOn(typeof(WebAppApplicationContractsModule))]
public class TaskyDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        Configure<TokenCleanupOptions>(options => options.IsCleanupEnabled = false);

        // Enable dynamic initializers in DbMigrator to seed permissions, features, and settings
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
        });

        Configure<FeatureManagementOptions>(options =>
        {
            options.IsDynamicFeatureStoreEnabled = true;
        });

        Configure<SettingManagementOptions>(options =>
        {
            options.IsDynamicSettingStoreEnabled = true;
        });
    }
}
