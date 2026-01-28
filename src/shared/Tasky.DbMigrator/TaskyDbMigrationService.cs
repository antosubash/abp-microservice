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
        // Check if we should reset (drop and recreate) databases
        var resetDatabases = Environment.GetEnvironmentVariable("RESET_DATABASES");
        if (resetDatabases?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            logger.LogWarning("RESET_DATABASES flag detected. Dropping all databases...");
            await DropDatabasesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("All databases dropped successfully.");
        }

        await CreateDatabasesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Starting Migrations ...");
        await MigrateHostAsync(cancellationToken).ConfigureAwait(false);
        await MigrateTenantsAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Completed Migrations.");
    }

    private async Task DropDatabasesAsync(CancellationToken cancellationToken)
    {
        using var uow = unitOfWorkManager.Begin(true);

        await DropDatabaseAsync<SaaSDbContext>(cancellationToken).ConfigureAwait(false);
        await DropDatabaseAsync<AdministrationDbContext>(cancellationToken).ConfigureAwait(false);
        await DropDatabaseAsync<IdentityServiceDbContext>(cancellationToken).ConfigureAwait(false);
        await DropDatabaseAsync<ProjectsDbContext>(cancellationToken).ConfigureAwait(false);

        await uow.CompleteAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task CreateDatabasesAsync(CancellationToken cancellationToken)
    {
        using var uow = unitOfWorkManager.Begin(true);

        await EnsureDatabaseAsync<SaaSDbContext>(cancellationToken).ConfigureAwait(false);
        await EnsureDatabaseAsync<AdministrationDbContext>(cancellationToken).ConfigureAwait(false);
        await EnsureDatabaseAsync<IdentityServiceDbContext>(cancellationToken).ConfigureAwait(false);

        await uow.CompleteAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task MigrateHostAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migrating Host side ...");
        await MigrateDatabasesAsync(null, cancellationToken).ConfigureAwait(false);
        await SeedDataAsync(null).ConfigureAwait(false);
        logger.LogInformation("Host side migration completed.");
    }

    private async Task MigrateTenantsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migrating tenants ...");

        var tenants = await tenantRepository
            .GetListAsync(includeDetails: true, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var migratedDatabaseSchemas = new HashSet<string>();

        foreach (var tenant in tenants)
        {
            using (currentTenant.Change(tenant.Id))
            {
                // Database schema migration
                var connectionString = tenant.FindDefaultConnectionString();
                if (
                    !connectionString.IsNullOrWhiteSpace()
                    && // tenant has a separate database
                    !migratedDatabaseSchemas.Contains(connectionString)
                ) // the database was not migrated yet
                {
                    logger.LogInformation("Migrating Tenant: {Name} ({TenantId})", tenant.Name, tenant.Id);

                    await MigrateDatabasesAsync(tenant, cancellationToken).ConfigureAwait(false);
                    migratedDatabaseSchemas.AddIfNotContains(connectionString);
                }

                // Seed data
                await SeedDataAsync(tenant).ConfigureAwait(false);
            }
        }

        logger.LogInformation("Tenant migrations are complete.");
    }

    private async Task DropDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var name = typeof(TDbContext).Name.RemovePostFix("DbContext");
        logger.LogInformation("Dropping {Name} database ...", name);

        var dbContext = await unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync()
            .ConfigureAwait(false);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        await strategy
            .ExecuteAsync(async () =>
            {
                if (await dbCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    await dbCreator.DeleteAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogInformation("Dropped {Name} database.", name);
                }
                else
                {
                    logger.LogInformation("{Name} database does not exist, skipping drop.", name);
                }
            })
            .ConfigureAwait(false);
    }

    private async Task EnsureDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var dbContext = await unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync()
            .ConfigureAwait(false);

        var strategy = dbContext.Database.CreateExecutionStrategy();

        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        await strategy
            .ExecuteAsync(async () =>
            {
                // Create the database if it does not exist.
                // Do this first so there is then a database to start a transaction against.
                if (!await dbCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    await dbCreator.CreateAsync(cancellationToken).ConfigureAwait(false);
                }
            })
            .ConfigureAwait(false);
    }

    private async Task MigrateDatabasesAsync(Tenant? tenant, CancellationToken cancellationToken)
    {
        using var uow = unitOfWorkManager.Begin(true);

        if (tenant is null)
        {
            /* SaaS schema should only be available in the host side */
            await MigrateDatabaseAsync<SaaSDbContext>(cancellationToken).ConfigureAwait(false);
        }

        await MigrateDatabaseAsync<AdministrationDbContext>(cancellationToken).ConfigureAwait(false);
        await MigrateDatabaseAsync<IdentityServiceDbContext>(cancellationToken).ConfigureAwait(false);
        await MigrateDatabaseAsync<ProjectsDbContext>(cancellationToken).ConfigureAwait(false);

        // await MigrateDatabaseAsync<WebAppDbContext>(cancellationToken);
        await uow.CompleteAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task MigrateDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var name = typeof(TDbContext).Name.RemovePostFix("DbContext");

        logger.LogInformation("Migrating {Name} database ...", name);

        var dbContext = await unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync()
            .ConfigureAwait(false);

        await ApplyMigrationAsync(dbContext, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Completed migrating ({Name}).", name);
    }

    private static Task ApplyMigrationAsync<TDbContext>(TDbContext dbContext, CancellationToken cancellationToken)
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
