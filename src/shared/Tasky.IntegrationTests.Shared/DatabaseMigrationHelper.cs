using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Tasky.IntegrationTests.Shared;

public class DatabaseMigrationHelper : ITransientDependency
{
    private const string AdminEmailPropertyName = "AdminEmail";
    private const string AdminPasswordPropertyName = "AdminPassword";

    private readonly ICurrentTenant _currentTenant;
    private readonly IDataSeeder _dataSeeder;
    private readonly ILogger<DatabaseMigrationHelper> _logger;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public DatabaseMigrationHelper(
        ILogger<DatabaseMigrationHelper> logger,
        IUnitOfWorkManager unitOfWorkManager,
        IDataSeeder dataSeeder,
        ICurrentTenant currentTenant
    )
    {
        _logger = logger;
        _unitOfWorkManager = unitOfWorkManager;
        _dataSeeder = dataSeeder;
        _currentTenant = currentTenant;
    }

    public async Task MigrateDatabaseAsync<TDbContext>(
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var name = typeof(TDbContext).Name.RemovePostFix("DbContext");
        _logger.LogInformation("Migrating {Name} database ...", name);

        var dbContext = await _unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });

        _logger.LogInformation("Completed migrating {Name}.", name);
    }

    public async Task SeedDataAsync(Guid? tenantId = null)
    {
        using var uow = _unitOfWorkManager.Begin(true);

        try
        {
            using (_currentTenant.Change(tenantId))
            {
                await _dataSeeder.SeedAsync(
                    new DataSeedContext(tenantId)
                        .WithProperty(AdminEmailPropertyName, IntegrationTestConstants.TestAdminEmail)
                        .WithProperty(
                            AdminPasswordPropertyName,
                            IntegrationTestConstants.TestAdminPassword
                        )
                );
            }

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error seeding data for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task EnsureDatabaseCreatedAsync<TDbContext>(
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var dbContext = await _unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }
}

