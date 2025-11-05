using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Tasky.Administration;

public class AdministrationIntegrationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });
    }
}

