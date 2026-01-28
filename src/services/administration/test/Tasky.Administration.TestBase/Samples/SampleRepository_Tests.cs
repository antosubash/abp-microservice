using Volo.Abp.Modularity;
using Xunit;

namespace Tasky.Administration.Samples;

/* Write your custom repository tests like that, in this project, as abstract classes.
 * Then inherit these abstract classes from EF Core & MongoDB test projects.
 * In this way, both database providers are tests with the same set tests.
 */
public abstract class SampleRepository_Tests<TStartupModule> : AdministrationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [Fact]
    public void Method1()
    {
        Assert.Fail("Not implemented!");
    }
}
