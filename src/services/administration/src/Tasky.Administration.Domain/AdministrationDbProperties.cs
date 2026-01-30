namespace Tasky.Administration;

public static class AdministrationDbProperties
{
    public const string ConnectionStringName = "Administration";

    public static string DbTablePrefix { get; set; } = "Administration";

    public static string DbSchema { get; set; } = null;
}
