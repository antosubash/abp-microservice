namespace Tasky.IntegrationTests.Shared;

public static class IntegrationTestConstants
{
    public const string TestDatabasePrefix = "test_";
    public const string TestPostgresImage = "postgres:16-alpine";
    public const string TestDatabaseUser = "postgres";
    public const string TestDatabasePassword = "postgres";
    public const string TestDatabaseName = "testdb";
    public const int TestContainerPort = 5432;

    public const string TestAuthServerUrl = "https://localhost:7600";
    public const string TestClientId = "IntegrationTest_Client";
    public const string TestClientSecret = "IntegrationTest_Secret";
    public const string TestScope = "offline_access Administration IdentityService Projects SaaS";

    public const string TestAdminEmail = "admin@test.com";
    public const string TestAdminPassword = "1q2w3E*";
}

