# Integration Tests Documentation

This document provides comprehensive information about the integration test infrastructure used across all microservices in this solution.

## Overview

The integration test framework provides a standardized way to test microservices end-to-end using:
- **Testcontainers** for isolated PostgreSQL database instances
- **WebApplicationFactory** for in-memory HTTP server testing
- **xUnit** as the test framework
- **Shouldly** for fluent assertions

## Architecture

### Shared Components

The `Tasky.IntegrationTests.Shared` project contains reusable components used by all service integration tests:

#### 1. IntegrationTestConstants

Centralized constants for test configuration:

```1:19:src/shared/Tasky.IntegrationTests.Shared/IntegrationTestConstants.cs
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
```

#### 2. TestcontainersPostgresFactory

Factory for creating PostgreSQL testcontainers:

```7:33:src/shared/Tasky.IntegrationTests.Shared/TestcontainersPostgresFactory.cs
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
```

#### 3. TestDatabaseMigrationHelper

Helper for running database migrations in test containers:

```6:45:src/shared/Tasky.IntegrationTests.Shared/TestDatabaseMigrationHelper.cs
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
```

#### 4. AuthenticationTestHelper

Helper for creating JWT tokens for authenticated test requests:

```8:65:src/shared/Tasky.IntegrationTests.Shared/AuthenticationTestHelper.cs
public static class AuthenticationTestHelper
{
    public static string CreateTestJwtToken(
        string? userName = null,
        string? email = null,
        Guid? tenantId = null,
        List<string>? roles = null
    )
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(userName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName));
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userName));
        }

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenantid", tenantId.Value.ToString()));
        }

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("IntegrationTestSecretKeyThatMustBeAtLeast32CharactersLong")
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: IntegrationTestConstants.TestAuthServerUrl,
            audience: IntegrationTestConstants.TestAuthServerUrl,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GetBearerTokenHeader(string token)
    {
        return $"Bearer {token}";
    }
}
```

## Service Integration Test Structure

Each microservice has its own integration test project following this structure:

```
Tasky.{ServiceName}.IntegrationTests/
├── Fixtures/
│   ├── TestcontainersFixture.cs          # Container setup and lifecycle
│   └── {ServiceName}WebApplicationFactory.cs  # Web host configuration
├── Samples/
│   ├── ApiEndpointTests.cs               # Example API tests
│   └── HealthCheckTests.cs               # Health check tests
├── {ServiceName}IntegrationTestBase.cs   # Base class for test classes
├── {ServiceName}IntegrationTestModule.cs # ABP module configuration
├── appsettings.Test.json                 # Test configuration
└── Tasky.{ServiceName}.IntegrationTests.csproj
```

### TestcontainersFixture

Each service defines its own `TestcontainersFixture` that:

1. Creates and starts PostgreSQL containers for required databases
2. Runs database migrations
3. Provides connection strings to tests
4. Cleans up containers after tests complete

Example from Administration service:

```13:74:src/services/administration/test/Tasky.Administration.IntegrationTests/Fixtures/TestcontainersFixture.cs
public class TestcontainersFixture : IAsyncLifetime
{
    private const string AdministrationDbName = "test_administration_db";
    private const string IdentityDbName = "test_identity_db";
    private readonly ILogger<TestcontainersFixture>? _logger;

    public TestcontainersFixture()
    {
        _logger = null;
    }

    public IContainer AdministrationDbContainer { get; private set; } = null!;
    public IContainer IdentityDbContainer { get; private set; } = null!;

    public string AdministrationConnectionString { get; private set; } = string.Empty;
    public string IdentityConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        AdministrationDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            AdministrationDbName
        );
        IdentityDbContainer = TestcontainersPostgresFactory.CreatePostgresContainer(
            IdentityDbName
        );

        await Task.WhenAll(
            AdministrationDbContainer.StartAsync(),
            IdentityDbContainer.StartAsync()
        );

        AdministrationConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            AdministrationDbContainer,
            AdministrationDbName
        );
        IdentityConnectionString = TestcontainersPostgresFactory.GetConnectionString(
            IdentityDbContainer,
            IdentityDbName
        );

        await Task.WhenAll(
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<AdministrationDbContext>(
                AdministrationConnectionString,
                _logger
            ),
            TestDatabaseMigrationHelper.MigrateDatabaseAsync<IdentityDbContext>(
                IdentityConnectionString,
                _logger
            )
        );
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(
            AdministrationDbContainer.StopAsync(),
            IdentityDbContainer.StopAsync()
        );
    }
}
```

### WebApplicationFactory

Each service has a custom `WebApplicationFactory` that:

1. Configures the test environment
2. Overrides connection strings with test container values
3. Sets up test-specific configuration

Example from Administration service:

```9:49:src/services/administration/test/Tasky.Administration.IntegrationTests/Fixtures/AdministrationWebApplicationFactory.cs
public class AdministrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestcontainersFixture _fixture;

    public AdministrationWebApplicationFactory(TestcontainersFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TaskyAdministrationDb"] =
                        _fixture.AdministrationConnectionString,
                    ["ConnectionStrings:TaskyIdentityServiceDb"] =
                        _fixture.IdentityConnectionString
                }
            );
        });

        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_fixture);
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.Timeout = TimeSpan.FromMinutes(5);
    }
}
```

### Integration Test Base Class

Each service provides a base class that all test classes inherit from:

```7:45:src/services/administration/test/Tasky.Administration.IntegrationTests/AdministrationIntegrationTestBase.cs
[Collection("IntegrationTests")]
public abstract class AdministrationIntegrationTestBase : IDisposable
{
    protected AdministrationWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }
    protected TestcontainersFixture Fixture { get; }

    protected AdministrationIntegrationTestBase(TestcontainersFixture fixture)
    {
        Fixture = fixture;
        Factory = new AdministrationWebApplicationFactory(fixture);
        Client = Factory.CreateClient();
    }

    protected string GetBearerTokenHeader(
        string? userName = null,
        string? email = null,
        Guid? tenantId = null,
        List<string>? roles = null
    )
    {
        var token = AuthenticationTestHelper.CreateTestJwtToken(userName, email, tenantId, roles);
        return AuthenticationTestHelper.GetBearerTokenHeader(token);
    }

    protected void SetAuthorizationHeader(HttpClient client, string bearerTokenHeader)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", bearerTokenHeader);
    }

    public virtual void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}
```

## Writing Integration Tests

### Basic Test Structure

```csharp
public class MyApiTests : AdministrationIntegrationTestBase
{
    public MyApiTests(TestcontainersFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Should_Return_Ok_For_EndpointAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
```

### Authenticated Requests

```csharp
[Fact]
public async Task Should_Return_Ok_For_Authorized_Endpoint_With_TokenAsync()
{
    var tokenHeader = GetBearerTokenHeader(
        userName: "testuser",
        email: IntegrationTestConstants.TestAdminEmail,
        roles: new List<string> { "admin" }
    );
    SetAuthorizationHeader(Client, tokenHeader);

    var response = await Client.GetAsync("/api/Administration/sample/authorized");
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
```

### Example Test Classes

See the `Samples` folder in each integration test project for complete examples:

- **HealthCheckTests**: Basic health check endpoint tests
- **ApiEndpointTests**: Example API endpoint tests with authentication

```11:47:src/services/administration/test/Tasky.Administration.IntegrationTests/Samples/ApiEndpointTests.cs
public class ApiEndpointTests : AdministrationIntegrationTestBase
{
    public ApiEndpointTests(TestcontainersFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Should_Return_Ok_For_Sample_EndpointAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Unauthorized_For_Authorized_Endpoint_Without_TokenAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample/authorized");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_Ok_For_Authorized_Endpoint_With_TokenAsync()
    {
        var tokenHeader = GetBearerTokenHeader(
            userName: "testuser",
            email: IntegrationTestConstants.TestAdminEmail
        );
        SetAuthorizationHeader(Client, tokenHeader);

        var response = await Client.GetAsync("/api/Administration/sample/authorized");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Json_Content_For_Sample_EndpointAsync()
    {
        var response = await Client.GetAsync("/api/Administration/sample");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldContain("application/json");
    }
}
```

## Running Integration Tests

For detailed instructions on running integration tests, including setup, troubleshooting, and advanced options, see the [Running Integration Tests Guide](running-integration-tests.md).

### Quick Start

**Prerequisites:**
- Docker Desktop must be running (for Testcontainers)
- .NET 9.0 SDK
- All NuGet packages restored

**Run All Integration Tests:**
```bash
dotnet test src/Tasky.sln --filter "FullyQualifiedName~IntegrationTests"
```

**Run Specific Service Integration Tests:**
```bash
dotnet test src/services/administration/test/Tasky.Administration.IntegrationTests
```

**Run a Specific Test Class:**
```bash
dotnet test --filter "FullyQualifiedName~ApiEndpointTests"
```

**Run a Specific Test Method:**
```bash
dotnet test --filter "FullyQualifiedName~Should_Return_Ok_For_Sample_EndpointAsync"
```

For more comprehensive instructions, including Visual Studio setup, CI/CD examples, and troubleshooting, refer to the [Running Integration Tests Guide](running-integration-tests.md).

## Available Services

The following services have integration test projects:

1. **Administration** (`Tasky.Administration.IntegrationTests`)
   - Tests: Administration and Identity databases
   
2. **Identity** (`Tasky.IdentityService.IntegrationTests`)
   - Tests: Administration, Identity, and SaaS databases

3. **Projects** (`Tasky.Projects.IntegrationTests`)
   - Tests: Administration and Projects databases

4. **SaaS** (`Tasky.SaaS.IntegrationTests`)
   - Tests: Administration and SaaS databases

## Configuration

### appsettings.Test.json

Each integration test project includes an `appsettings.Test.json` file with test-specific configuration:

```1:21:src/services/administration/test/Tasky.Administration.IntegrationTests/appsettings.Test.json
{
  "App": {
    "CorsOrigins": "*"
  },
  "AuthServer": {
    "Authority": "https://localhost:7600/",
    "RequireHttpsMetadata": "false",
    "SwaggerClientId": "Administration_API",
    "SwaggerClientSecret": "1q2w3e*"
  },
  "RabbitMQ": {
    "EventBus": {
      "ClientName": "Tasky_Administration_Test",
      "ExchangeName": "Tasky"
    }
  },
  "ConnectionStrings": {
    "TaskyAdministrationDb": "",
    "TaskyIdentityServiceDb": ""
  }
}
```

Connection strings are automatically overridden by the `WebApplicationFactory` with values from test containers.

## Best Practices

1. **Use the base class**: Always inherit from `{ServiceName}IntegrationTestBase` for consistent setup
2. **Use constants**: Use `IntegrationTestConstants` instead of hardcoded strings
3. **Clean up**: The base class handles disposal automatically, but clean up test data if needed
4. **Isolated tests**: Each test should be independent and not rely on other tests
5. **Use descriptive names**: Follow the pattern `Should_{ExpectedBehavior}_When_{Condition}Async`
6. **Test authentication**: Always test both authenticated and unauthenticated scenarios
7. **Assert thoroughly**: Use Shouldly for readable assertions

## Troubleshooting

### Docker Not Running

If Docker Desktop is not running, tests will fail with container startup errors. Ensure Docker Desktop is running before executing tests.

### Port Conflicts

If you see port binding errors, ensure no other services are using the ports. Testcontainers automatically assigns random ports, but conflicts can still occur.

### Migration Failures

If migrations fail, check:
- Database context constructor signature matches expected pattern
- All required migrations are included in the project
- Connection string format is correct

### Authentication Issues

If authentication tests fail:
- Verify JWT token creation includes all required claims
- Check that the test auth server configuration matches expectations
- Ensure the test environment is properly configured

## Dependencies

Key NuGet packages used:

- `Microsoft.AspNetCore.Mvc.Testing` - WebApplicationFactory support
- `xunit` - Test framework
- `Shouldly` - Fluent assertions
- `Testcontainers.PostgreSql` - PostgreSQL testcontainers
- `Microsoft.EntityFrameworkCore` - Database access
- `Volo.Abp.TestBase` - ABP testing utilities

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Shouldly Documentation](https://docs.shouldly.org/)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [ASP.NET Core Integration Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
