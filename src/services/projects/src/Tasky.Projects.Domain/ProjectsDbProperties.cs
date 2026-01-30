namespace Tasky.Projects;

public static class ProjectsDbProperties
{
    public const string ConnectionStringName = "Projects";

    public static string DbTablePrefix { get; set; } = "Projects";

    public static string DbSchema { get; set; } = null;
}
