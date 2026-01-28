using System.Diagnostics;
using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projects;

namespace Tasky.AppHost;

internal class Program
{
    private static void Main(string[] args)
    {
        const string LaunchProfileName = "Aspire";
        var builder = DistributedApplication.CreateBuilder(args);

        var postgres = builder.AddPostgres(TaskyNames.Postgres).WithPgWeb();
        var rabbitMq = builder.AddRabbitMQ(TaskyNames.RabbitMq).WithManagementPlugin();
        var redis = builder.AddRedis(TaskyNames.Redis).WithRedisCommander();
        var seq = builder.AddSeq(TaskyNames.Seq);

        var adminDb = postgres.AddDatabase(TaskyNames.AdministrationDb);
        var identityDb = postgres.AddDatabase(TaskyNames.IdentityServiceDb);
        var projectsDb = postgres.AddDatabase(TaskyNames.ProjectsDb);
        var saasDb = postgres.AddDatabase(TaskyNames.SaaSDb);

        var migrator = builder
            .AddProject<Tasky_DbMigrator>(TaskyNames.DbMigrator, launchProfileName: LaunchProfileName)
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(projectsDb)
            .WithReference(saasDb)
            .WithReference(seq)
            .WaitFor(postgres)
            .WithCommand(
                name: "reset-databases",
                displayName: "Reset Databases",
                executeCommand: async context =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Starting database reset - dropping and recreating all databases...");

                    try
                    {
                        // Get the migrator project directory
                        var appHostDir = AppContext.BaseDirectory;
                        var migratorDir = Path.GetFullPath(
                            Path.Combine(appHostDir, "..", "..", "..", "..", "..", "shared", "Tasky.DbMigrator")
                        );

                        logger.LogInformation("DbMigrator directory: {MigratorDir}", migratorDir);

                        // Start the process
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "run --configuration Release",
                            WorkingDirectory = migratorDir,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                        };

                        // Set environment variable
                        processInfo.Environment["RESET_DATABASES"] = "true";

                        // Copy connection strings from the migrator resource environment
                        processInfo.Environment["ConnectionStrings__TaskyAdministrationDb"] = adminDb
                            .Resource
                            .ConnectionStringExpression
                            .ValueExpression;
                        processInfo.Environment["ConnectionStrings__TaskyIdentityServiceDb"] = identityDb
                            .Resource
                            .ConnectionStringExpression
                            .ValueExpression;
                        processInfo.Environment["ConnectionStrings__TaskyProjectsDb"] = projectsDb
                            .Resource
                            .ConnectionStringExpression
                            .ValueExpression;
                        processInfo.Environment["ConnectionStrings__TaskySaaSDb"] = saasDb
                            .Resource
                            .ConnectionStringExpression
                            .ValueExpression;

                        using var process = new Process { StartInfo = processInfo };

                        process.OutputDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                logger.LogInformation("[DbMigrator] {Output}", args.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                logger.LogError("[DbMigrator] {Error}", args.Data);
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await process.WaitForExitAsync(context.CancellationToken).ConfigureAwait(false);

                        if (process.ExitCode == 0)
                        {
                            logger.LogInformation("Database reset completed successfully!");
                            return CommandResults.Success();
                        }
                        else
                        {
                            logger.LogError("Database reset failed with exit code {ExitCode}", process.ExitCode);
                            return CommandResults.Failure("Database reset failed. Check logs for details.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error executing database reset command");
                        return CommandResults.Failure($"Error: {ex.Message}");
                    }
                },
                commandOptions: new()
                {
                    IconName = "DatabaseArrowDown",
                    IconVariant = IconVariant.Filled,
                    IsHighlighted = true,
                }
            );

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
            .WaitFor(rabbitMq)
            .WaitFor(redis)
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
            .WaitFor(rabbitMq)
            .WaitFor(redis)
            .WaitForCompletion(migrator);

        var saas = builder
            .AddProject<Tasky_SaaS_HttpApi_Host>(TaskyNames.SaaSApi, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitFor(rabbitMq)
            .WaitFor(redis)
            .WaitForCompletion(migrator);

        builder
            .AddProject<Tasky_Projects_HttpApi_Host>(TaskyNames.ProjectsApi, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(projectsDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitFor(rabbitMq)
            .WaitFor(redis)
            .WaitForCompletion(migrator);

        var gateway = builder
            .AddProject<Tasky_Gateway>(TaskyNames.Gateway, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(admin)
            .WaitFor(identity)
            .WaitFor(saas);

        var authserver = builder
            .AddProject<Tasky_AuthServer>(TaskyNames.AuthServer, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitFor(rabbitMq)
            .WaitFor(redis)
            .WaitForCompletion(migrator);

        builder
            .AddProject<Tasky_WebApp_Blazor>(TaskyNames.WebAppClient, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(authserver)
            .WaitFor(gateway);

        builder.Build().Run();
    }
}
