using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Tasky.Administration.EntityFrameworkCore;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;

namespace Tasky;

[DependsOn(typeof(AbpAspNetCoreModule))]
[DependsOn(typeof(AbpAspNetCoreAuthenticationJwtBearerModule))]
[DependsOn(typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpSwashbuckleModule))]
[DependsOn(typeof(TaskyHostingModule))]
[DependsOn(typeof(AdministrationEntityFrameworkCoreModule))]
public class TaskyMicroserviceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // var hostingEnvironment = context.Services.GetHostingEnvironment();
        ConfigureCors(context, configuration);
    }

    private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]!
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}

public static class MicroserviceExtensions
{
    public static ServiceConfigurationContext ConfigureMicroservice(
        this ServiceConfigurationContext context,
        string name
    )
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        context.ConfigureAuthentication(configuration, name);
        context.ConfigureCache(name);
        context.ConfigureDataProtection(hostingEnvironment, configuration, name);
        context.ConfigureSwaggerServices(configuration, name);

        return context;
    }

    public static ServiceConfigurationContext ConfigureAuthentication(
        this ServiceConfigurationContext context,
        IConfiguration configuration,
        string audience
    )
    {
        context
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddAbpJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.Audience = audience;
            });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options => options.IsDynamicClaimsEnabled = true);

        return context;
    }

    public static ServiceConfigurationContext ConfigureCache(this ServiceConfigurationContext context, string keyPrefix)
    {
        context.Services.Configure<AbpDistributedCacheOptions>(options => options.KeyPrefix = $"{keyPrefix}:");

        return context;
    }

    public static ServiceConfigurationContext ConfigureSwaggerServices(
        this ServiceConfigurationContext context,
        IConfiguration configuration,
        string name,
        string version = "v1"
    )
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string> { { name, $"{name} API" } },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = $"{name} API", Version = version });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );

        return context;
    }
}
