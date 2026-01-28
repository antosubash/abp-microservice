using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Tasky.IdentityService.EventHandler;

public class TenantCreatedEventHandler(
    ICurrentTenant currentTenant,
    IIdentityDataSeeder identityDataSeeder,
    ILogger<TenantCreatedEventHandler> logger
) : IDistributedEventHandler<TenantCreatedEto>, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly ILogger<TenantCreatedEventHandler> _logger = logger;
    private readonly IIdentityDataSeeder _identityDataSeeder = identityDataSeeder;

    public async Task HandleEventAsync(TenantCreatedEto eventData)
    {
        try
        {
            using (_currentTenant.Change(eventData.Id))
            {
                _logger.LogInformation("Creating admin user for tenant {TenantId}...", eventData.Id);
                await _identityDataSeeder
                    .SeedAsync(
                        eventData.Properties.GetOrDefault(IdentityDataSeedContributor.AdminEmailPropertyName)
                            ?? "admin@antosubash.com",
                        eventData.Properties.GetOrDefault(IdentityDataSeedContributor.AdminPasswordPropertyName)
                            ?? "1q2w3E*",
                        eventData.Id
                    )
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await HandleErrorTenantCreatedAsync(eventData, ex).ConfigureAwait(false);
        }
    }

    private static Task HandleErrorTenantCreatedAsync(TenantCreatedEto eventData, Exception ex)
    {
        throw new NotImplementedException();
    }
}
