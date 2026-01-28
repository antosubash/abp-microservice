using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Tasky.Projects;

[DependsOn(typeof(ProjectsDomainModule))]
[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpAutoMapperModule))]
public class ProjectsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ProjectsApplicationModule>();
        Configure<AbpAutoMapperOptions>(options => options.AddMaps<ProjectsApplicationModule>(true));
    }
}
