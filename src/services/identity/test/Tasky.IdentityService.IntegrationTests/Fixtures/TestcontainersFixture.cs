using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.IdentityService.EntityFrameworkCore;
using Tasky.IntegrationTests.Shared;
using Tasky.SaaS.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Xunit;

namespace Tasky.IdentityService.Fixtures;

[CollectionDefinition("IntegrationTests")]
public class TestcontainersCollection : ICollectionFixture<TestcontainersFixture> { }

public class TestcontainersFixture : IAsyncLifetime
{
    private const string AdministrationDbName = "test_administration_db";
    private const string IdentityDbName = "test_identity_db";
    private const string SaaSDbName = "test_saas_db";
    private readonly ILogger<TestcontainersFixture>? _logger;

    public TestcontainersFixture()
    {
        _logger = null;
    }

    public IContainer AdministrationDbContainer { get; private set; } = null!;
    public IContainer IdentityDbContainer { get; private set; } = null!;
    public IContainer SaaSDbContainer { get; private set; } = null!;

    public string AdministrationConnectionString { get; private set; } = string.Empty;
    public string IdentityConnectionString { get; private set; } = string.Empty;
    public string SaaSConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        AdministrationDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            AdministrationDbName
        );
        IdentityDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            IdentityDbName
        );
        SaaSDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(SaaSDbName);

        await Task.WhenAll(
            AdministrationDbContainer.StartAsync(),
            IdentityDbContainer.StartAsync(),
            SaaSDbContainer.StartAsync()
        );

        AdministrationConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            AdministrationDbContainer,
            AdministrationDbName
        );
        IdentityConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            IdentityDbContainer,
            IdentityDbName
        );
        SaaSConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            SaaSDbContainer,
            SaaSDbName
        );

        await Task.WhenAll(
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<AdministrationDbContext>(
                AdministrationConnectionString,
                _logger
            ),
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<IdentityServiceDbContext>(
                IdentityConnectionString,
                _logger
            ),
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<SaaSDbContext>(
                SaaSConnectionString,
                _logger
            )
        );
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
            AdministrationDbContainer.StopAsync(),
            IdentityDbContainer.StopAsync(),
            SaaSDbContainer.StopAsync()
        );
    }
}

