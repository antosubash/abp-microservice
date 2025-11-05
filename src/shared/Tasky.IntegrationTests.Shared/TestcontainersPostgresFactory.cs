using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Tasky.IntegrationTests.Shared;

public static class TestcontainersPostgresFactory
{
    public static IContainer CreatePostgresContainer(string databaseName)
    {
        return new ContainerBuilder()
            .WithImage(IntegrationTestConstants.TestPostgresImage)
            .WithEnvironment("POSTGRES_USER", IntegrationTestConstants.TestDatabaseUser)
            .WithEnvironment("POSTGRES_PASSWORD", IntegrationTestConstants.TestDatabasePassword)
            .WithEnvironment("POSTGRES_DB", databaseName)
            .WithPortBinding(IntegrationTestConstants.TestContainerPort, true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilPortIsAvailable(IntegrationTestConstants.TestContainerPort)
            )
            .WithAutoRemove(true)
            .Build();
    }

    public static string GetConnectionString(IContainer container, string databaseName)
    {
        var host = container.Hostname;
        var port = container.GetMappedPublicPort(IntegrationTestConstants.TestContainerPort);

        return
            $"Host={host};Port={port};Database={databaseName};Username={IntegrationTestConstants.TestDatabaseUser};Password={IntegrationTestConstants.TestDatabasePassword}";
    }
}

