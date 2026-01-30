using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace Tasky.Administration.EventHandler;

public class TenantCreatedEventHandler(
    ICurrentTenant currentTenant,
    IUnitOfWorkManager unitOfWorkManager,
    IPermissionDefinitionManager permissionDefinitionManager,
    IPermissionDataSeeder permissionDataSeeder,
    ILogger<TenantCreatedEventHandler> logger
) : IDistributedEventHandler<TenantCreatedEto>, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly ILogger<TenantCreatedEventHandler> _logger = logger;
    private readonly IPermissionDataSeeder _permissionDataSeeder = permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager = permissionDefinitionManager;

    private readonly IUnitOfWorkManager _unitOfWorkManager = unitOfWorkManager;

    public async Task HandleEventAsync(TenantCreatedEto eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        try
        {
            await SeedDataAsync(eventData.Id).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorTenantCreatedAsync(eventData, ex).ConfigureAwait(false);
        }
    }

    private static Task HandleErrorTenantCreatedAsync(TenantCreatedEto eventData, Exception ex)
    {
        throw new NotImplementedException();
    }

    private async Task SeedDataAsync(Guid? tenantId)
    {
        _logger.LogInformation("Seeding ${TenantId}", tenantId);
        using (_currentTenant.Change(tenantId))
        {
            var abpUnitOfWorkOptions = new AbpUnitOfWorkOptions { IsTransactional = true };
            using var uow = _unitOfWorkManager.Begin(abpUnitOfWorkOptions, true);
            var multiTenancySide = tenantId is null ? MultiTenancySides.Host : MultiTenancySides.Tenant;

            var permissions = await _permissionDefinitionManager.GetPermissionsAsync().ConfigureAwait(false);

            var permissionNames = permissions
                .Where(p => p.MultiTenancySide.HasFlag(multiTenancySide))
                .Where(p => p.Providers.Count == 0 || p.Providers.Contains(RolePermissionValueProvider.ProviderName))
                .Select(p => p.Name)
                .ToArray();

            await _permissionDataSeeder
                .SeedAsync(RolePermissionValueProvider.ProviderName, "admin", permissionNames, tenantId)
                .ConfigureAwait(false);

            await uow.CompleteAsync().ConfigureAwait(false);
        }
    }
}
