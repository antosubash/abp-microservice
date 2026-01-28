using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Tasky.DbMigrator;

public class OpenIddictDataSeedContributor(OpenIddictDataSeeder openIddictDataSeeder)
    : IDataSeedContributor,
        ITransientDependency
{
    private readonly OpenIddictDataSeeder _openIddictDataSeeder = openIddictDataSeeder;

    public Task SeedAsync(DataSeedContext context)
    {
        return _openIddictDataSeeder.SeedAsync();
    }
}
