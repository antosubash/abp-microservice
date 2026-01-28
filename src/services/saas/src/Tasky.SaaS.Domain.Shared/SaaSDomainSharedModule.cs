using Tasky.SaaS.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Tasky.SaaS;

[DependsOn(typeof(AbpValidationModule))]
[DependsOn(typeof(AbpTenantManagementDomainSharedModule))]
public class SaaSDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<SaaSDomainSharedModule>());

        Configure<AbpLocalizationOptions>(options =>
        {
            options
                .Resources.Add<SaaSResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/SaaS");
        });

        Configure<AbpExceptionLocalizationOptions>(options => options.MapCodeNamespace("SaaS", typeof(SaaSResource)));
    }
}
