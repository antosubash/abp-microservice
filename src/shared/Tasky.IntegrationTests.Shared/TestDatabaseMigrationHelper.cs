using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tasky.IntegrationTests.Shared;

public static class TestDatabaseMigrationHelper
{
    public static async Task MigrateDatabaseAsync<TDbContext>(
        string connectionString,
        ILogger? logger = null
    )
        where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        var contextType = typeof(TDbContext);
        var constructor = contextType.GetConstructor(new[] { typeof(DbContextOptions<TDbContext>) });

        if (constructor == null)
        {
            throw new InvalidOperationException(
                $"No constructor found for {contextType.Name} accepting DbContextOptions<{contextType.Name}>"
            );
        }

        var context = (TDbContext)constructor.Invoke(new object[] { optionsBuilder.Options });

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            var name = contextType.Name.RemovePostFix("DbContext");

            logger?.LogInformation("Migrating {Name} database ...", name);

            await strategy.ExecuteAsync(async () => { await context.Database.MigrateAsync(); });

            logger?.LogInformation("Completed migrating {Name}.", name);
        }
        finally
        {
            await context.DisposeAsync();
        }
    }
}

