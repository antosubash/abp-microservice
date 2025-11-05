using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Tasky.Administration.EntityFrameworkCore;
using Tasky.IntegrationTests.Shared;
using Tasky.Projects.EntityFrameworkCore;
using Xunit;

namespace Tasky.Projects.Fixtures;

[CollectionDefinition("IntegrationTests")]
public class TestcontainersCollection : ICollectionFixture<TestcontainersFixture> { }

public class TestcontainersFixture : IAsyncLifetime
{
    private const string AdministrationDbName = "test_administration_db";
    private const string ProjectsDbName = "test_projects_db";
    private readonly ILogger<TestcontainersFixture>? _logger;

    public TestcontainersFixture()
    {
        _logger = null;
    }

    public IContainer AdministrationDbContainer { get; private set; } = null!;
    public IContainer ProjectsDbContainer { get; private set; } = null!;

    public string AdministrationConnectionString { get; private set; } = string.Empty;
    public string ProjectsConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        AdministrationDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            AdministrationDbName
        );
        ProjectsDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            ProjectsDbName
        );

        await Task.WhenAll(
            AdministrationDbContainer.StartAsync(),
            ProjectsDbContainer.StartAsync()
        );

        AdministrationConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            AdministrationDbContainer,
            AdministrationDbName
        );
        ProjectsConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            ProjectsDbContainer,
            ProjectsDbName
        );

        await Task.WhenAll(
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<AdministrationDbContext>(
                AdministrationConnectionString,
                _logger
            ),
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<ProjectsDbContext>(
                ProjectsConnectionString,
                _logger
            )
        );
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
            AdministrationDbContainer.StopAsync(),
            ProjectsDbContainer.StopAsync()
        );
    }
}

