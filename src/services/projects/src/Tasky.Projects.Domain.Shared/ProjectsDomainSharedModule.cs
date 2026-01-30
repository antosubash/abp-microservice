using Tasky.Projects.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Tasky.Projects;

[DependsOn(typeof(AbpValidationModule))]
public class ProjectsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<ProjectsDomainSharedModule>());

        Configure<AbpLocalizationOptions>(options =>
        {
            options
                .Resources.Add<ProjectsResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Projects");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
            options.MapCodeNamespace("Projects", typeof(ProjectsResource))
        );
    }
}
