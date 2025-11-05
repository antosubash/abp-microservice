using Microsoft.Extensions.Hosting;
using Projects;

namespace Tasky.AppHost;

public class Program
{
    private static void Main(string[] args)
    {
        const string LaunchProfileName = "Aspire";
        var builder = DistributedApplication.CreateBuilder(args);

        var postgres = builder.AddPostgres(TaskyNames.Postgres).WithPgWeb();
        var rabbitMq = builder.AddRabbitMQ(TaskyNames.RabbitMq).WithManagementPlugin();
        var redis = builder.AddRedis(TaskyNames.Redis).WithRedisCommander();
        var seq = builder
            .AddSeq(TaskyNames.Seq)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithDataVolume();

        var adminDb = postgres.AddDatabase(TaskyNames.AdministrationDb);
        var identityDb = postgres.AddDatabase(TaskyNames.IdentityServiceDb);
        var projectsDb = postgres.AddDatabase(TaskyNames.ProjectsDb);
        var saasDb = postgres.AddDatabase(TaskyNames.SaaSDb);

        var migrator = builder
            .AddProject<Tasky_DbMigrator>(
                TaskyNames.DbMigrator,
                launchProfileName: LaunchProfileName
            )
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(projectsDb)
            .WithReference(saasDb)
            .WithReference(seq)
            .WaitFor(postgres);

        var admin = builder
            .AddProject<Tasky_Administration_HttpApi_Host>(
                TaskyNames.AdministrationApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var identity = builder
            .AddProject<Tasky_IdentityService_HttpApi_Host>(
                TaskyNames.IdentityServiceApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var saas = builder
            .AddProject<Tasky_SaaS_HttpApi_Host>(
                TaskyNames.SaaSApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<Tasky_Projects_HttpApi_Host>(
                TaskyNames.ProjectsApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(projectsDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var gateway = builder
            .AddProject<Tasky_Gateway>(TaskyNames.Gateway, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(admin)
            .WaitFor(identity)
            .WaitFor(saas);

        var authserver = builder
            .AddProject<Tasky_AuthServer>(
                TaskyNames.AuthServer,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<Tasky_WebApp_Blazor>(
                TaskyNames.WebAppClient,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(authserver)
            .WaitFor(gateway);

        builder.Build().Run();
    }
}
