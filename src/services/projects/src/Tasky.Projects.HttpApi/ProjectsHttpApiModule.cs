using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Tasky.Projects.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace Tasky.Projects;

[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class ProjectsHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ProjectsHttpApiModule).Assembly)
        );
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
            options.Resources.Get<ProjectsResource>().AddBaseTypes(typeof(AbpUiResource))
        );
    }
}
