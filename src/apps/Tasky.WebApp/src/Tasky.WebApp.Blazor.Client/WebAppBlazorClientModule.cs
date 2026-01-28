using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Tasky.WebApp.Blazor.Client.Menus;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Components.WebAssembly.LeptonXLiteTheme;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Blazor.WebAssembly;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Blazor.WebAssembly;
using Volo.Abp.TenantManagement.Blazor.WebAssembly;
using Volo.Abp.UI.Navigation;

namespace Tasky.WebApp.Blazor.Client;

[DependsOn(typeof(AbpAspNetCoreComponentsWebAssemblyLeptonXLiteThemeModule))]
[DependsOn(typeof(AbpAutofacWebAssemblyModule))]
[DependsOn(typeof(AbpIdentityBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpSettingManagementBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpTenantManagementBlazorWebAssemblyModule))]
[DependsOn(typeof(TaskyWebAppDomainSharedModule))]
[DependsOn(typeof(TaskySharedModule))]
public class WebAppBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureAutoMapper();
        ConfigureBlazorise(context);
        ConfigureHttpClient(context, environment);
        ConfigureMenu();
        ConfigureRouter();
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("AuthServer", options.ProviderOptions);
            options.UserOptions.NameClaim = OpenIddictConstants.Claims.Name;
            options.UserOptions.RoleClaim = OpenIddictConstants.Claims.Role;
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add("roles");
            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add("phone");
            options.ProviderOptions.DefaultScopes.Add(TaskyNames.AdministrationApi);
            options.ProviderOptions.DefaultScopes.Add(TaskyNames.IdentityServiceApi);
            options.ProviderOptions.DefaultScopes.Add(TaskyNames.SaaSApi);
            options.ProviderOptions.DefaultScopes.Add(TaskyNames.WebAppApi);
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options => options.AddMaps<WebAppBlazorClientModule>());
    }

    private static void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context.Services.AddBootstrap5Providers().AddFontAwesomeIcons();
    }

    private static void ConfigureHttpClient(
        ServiceConfigurationContext context,
        IWebAssemblyHostEnvironment environment
    )
    {
        context.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(environment.BaseAddress) });
    }

    private void ConfigureMenu()
    {
        Configure<AbpNavigationOptions>(options => options.MenuContributors.Add(new WebAppMenuContributor()));
    }

    private void ConfigureRouter()
    {
        Configure<AbpRouterOptions>(options => options.AppAssembly = typeof(WebAppBlazorClientModule).Assembly);
    }
}
