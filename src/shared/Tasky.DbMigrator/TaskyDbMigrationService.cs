using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.IdentityService.EntityFrameworkCore;
using Tasky.Projects.EntityFrameworkCore;
using Tasky.SaaS.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Tasky.DbMigrator;

public class TaskyDbMigrationService(
    ILogger<TaskyDbMigrationService> logger,
    ITenantRepository tenantRepository,
    IDataSeeder dataSeeder,
    ICurrentTenant currentTenant,
    IUnitOfWorkManager unitOfWorkManager
) : ITransientDependency
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await CreateDatabasesAsync(cancellationToken);

        logger.LogInformation("Starting Migrations ...");
        await MigrateHostAsync(cancellationToken);
        await MigrateTenantsAsync(cancellationToken);
        logger.LogInformation("Completed Migrations.");
    }

    private async Task CreateDatabasesAsync(CancellationToken cancellationToken)
    {
        using var uow = unitOfWorkManager.Begin(true);

        await EnsureDatabaseAsync<SaaSDbContext>(cancellationToken);
        await EnsureDatabaseAsync<AdministrationDbContext>(cancellationToken);
        await EnsureDatabaseAsync<IdentityServiceDbContext>(cancellationToken);

        await uow.CompleteAsync(cancellationToken);
    }

    private async Task MigrateHostAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migrating Host side ...");
        await MigrateDatabasesAsync(null, cancellationToken);
        await SeedDataAsync(null);
        logger.LogInformation("Host side migration completed.");
    }

    private async Task MigrateTenantsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migrating tenants ...");

        var tenants = await tenantRepository.GetListAsync(
            includeDetails: true,
            cancellationToken: cancellationToken
        );
        var migratedDatabaseSchemas = new HashSet<string>();

        foreach (var tenant in tenants)
        {
            using (currentTenant.Change(tenant.Id))
            {
                // Database schema migration
                var connectionString = tenant.FindDefaultConnectionString();
                if (
                    !connectionString.IsNullOrWhiteSpace()
                    && //tenant has a separate database
                    !migratedDatabaseSchemas.Contains(connectionString)
                ) //the database was not migrated yet
                {
                    logger.LogInformation(
                        "Migrating Tenant: {Name} ({TenantId})",
                        tenant.Name,
                        tenant.Id
                    );

                    await MigrateDatabasesAsync(tenant, cancellationToken);
                    migratedDatabaseSchemas.AddIfNotContains(connectionString);
                }

                //Seed data
                await SeedDataAsync(tenant);
            }
        }

        logger.LogInformation("Tenant migrations are complete.");
    }

    private async Task EnsureDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var dbContext = await unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private async Task MigrateDatabasesAsync(Tenant? tenant, CancellationToken cancellationToken)
    {
        using var uow = unitOfWorkManager.Begin(true);

        if (tenant is null)
        {
            /* SaaS schema should only be available in the host side */
            await MigrateDatabaseAsync<SaaSDbContext>(cancellationToken);
        }

        await MigrateDatabaseAsync<AdministrationDbContext>(cancellationToken);
        await MigrateDatabaseAsync<IdentityServiceDbContext>(cancellationToken);
        await MigrateDatabaseAsync<ProjectsDbContext>(cancellationToken);
        //await MigrateDatabaseAsync<WebAppDbContext>(cancellationToken);

        await uow.CompleteAsync(cancellationToken);
    }

    private async Task MigrateDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var name = typeof(TDbContext).Name.RemovePostFix("DbContext");

        logger.LogInformation("Migrating {Name} database ...", name);

        var dbContext = await unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        await ApplyMigrationAsync(dbContext, cancellationToken);

        logger.LogInformation("Completed migrating ({Name}).", name);
    }

    private static Task ApplyMigrationAsync<TDbContext>(
        TDbContext dbContext,
        CancellationToken cancellationToken
    )
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(() => dbContext.Database.MigrateAsync(cancellationToken));
    }

    private Task SeedDataAsync(Tenant? tenant)
    {
        if (tenant is null)
        {
            logger.LogInformation("Seeding host data ...");
        }
        else
        {
            logger.LogInformation("Seeding tenant data: {Name} ({Id})", tenant.Name, tenant.Id);
        }

        return dataSeeder.SeedAsync(
            new DataSeedContext(tenant?.Id)
                .WithProperty(
                    IdentityDataSeedContributor.AdminEmailPropertyName,
                    IdentityDataSeedContributor.AdminEmailDefaultValue
                )
                .WithProperty(
                    IdentityDataSeedContributor.AdminPasswordPropertyName,
                    IdentityDataSeedContributor.AdminPasswordDefaultValue
                )
        );
    }
}
