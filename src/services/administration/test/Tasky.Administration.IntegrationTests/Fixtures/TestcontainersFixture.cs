using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.IntegrationTests.Shared;
using Volo.Abp.Identity.EntityFrameworkCore;
using Xunit;

namespace Tasky.Administration.Fixtures;

[CollectionDefinition("IntegrationTests")]
public class TestcontainersCollection : ICollectionFixture<TestcontainersFixture> { }

public class TestcontainersFixture : IAsyncLifetime
{
    private const string AdministrationDbName = "test_administration_db";
    private const string IdentityDbName = "test_identity_db";
    private readonly ILogger<TestcontainersFixture>? _logger;

    public TestcontainersFixture()
    {
        _logger = null;
    }

    public IContainer AdministrationDbContainer { get; private set; } = null!;
    public IContainer IdentityDbContainer { get; private set; } = null!;

    public string AdministrationConnectionString { get; private set; } = string.Empty;
    public string IdentityConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        AdministrationDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            AdministrationDbName
        );
        IdentityDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            IdentityDbName
        );

        await Task.WhenAll(
            AdministrationDbContainer.StartAsync(),
            IdentityDbContainer.StartAsync()
        );

        AdministrationConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            AdministrationDbContainer,
            AdministrationDbName
        );
        IdentityConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            IdentityDbContainer,
            IdentityDbName
        );

        await Task.WhenAll(
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<AdministrationDbContext>(
                AdministrationConnectionString,
                _logger
            ),
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<IdentityDbContext>(
                IdentityConnectionString,
                _logger
            )
        );
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
            AdministrationDbContainer.StopAsync(),
            IdentityDbContainer.StopAsync()
        );
    }
}

