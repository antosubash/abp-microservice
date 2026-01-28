using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.VirtualFileSystem;

namespace Tasky.Administration;

[DependsOn(typeof(AdministrationApplicationContractsModule))]
[DependsOn(typeof(AbpHttpClientModule))]
[DependsOn(typeof(AbpPermissionManagementHttpApiClientModule))]
[DependsOn(typeof(AbpSettingManagementHttpApiClientModule))]
[DependsOn(typeof(AbpFeatureManagementHttpApiClientModule))]
public class AdministrationHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Services.AddHttpClientProxies(
            typeof(AdministrationApplicationContractsModule).Assembly,
            AdministrationRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
            options.FileSets.AddEmbedded<AdministrationHttpApiClientModule>()
        );
    }
}
