# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ABP Framework-based microservices template using .NET Aspire for orchestration. The solution demonstrates a production-grade microservices architecture with four core services (Administration, Identity, Projects, SaaS), a YARP-based API gateway, OpenIddict authentication server, and Blazor WebAssembly frontend.

**Current ABP Version:** 10.0
**Target Framework:** .NET 10.0
**.NET Aspire Version:** 9.5.2

## Quick Start with Makefile

A comprehensive Makefile is available at the repository root for common development tasks:

```bash
# First-time setup
make install          # Restore tools and install git hooks

# Daily workflow
make build            # Build the solution
make test             # Run all tests
make format           # Format code with CSharpier
make fix              # Auto-fix analyzers + style + format
make run              # Run with .NET Aspire orchestration

# Database operations
make migrate          # Run database migrations
make reset-db         # Reset databases (dev only - DESTRUCTIVE)

# Individual services
make run-gateway      # Run API Gateway only
make run-auth         # Run AuthServer only
make run-admin        # Run Administration service only

# Code quality
make check-format     # Check formatting without changes
make warnings         # Show build warnings/errors
make analyzers        # Show analyzer diagnostics

# Help
make help             # Show all available targets
make dev-help         # Show quick start guide
```

All commands below can also be run using the Makefile shortcuts shown above.

## Development Commands

### Building the Solution

```bash
# Build entire solution from src/ directory
dotnet build Tasky.sln

# Build specific service
dotnet build services/administration/host/Tasky.Administration.HttpApi.Host/Tasky.Administration.HttpApi.Host.csproj

# Clean build artifacts
clean.bat  # or manually: dotnet clean
```

### Running the Application

```bash
# Run with .NET Aspire orchestration (recommended)
cd apps/Tasky.AppHost
dotnet run

# This starts:
# - PostgreSQL (with 4 databases)
# - RabbitMQ (with management UI)
# - Redis (with Redis Commander)
# - Seq (log aggregation)
# - DbMigrator (runs migrations)
# - All microservices (Administration, Identity, Projects, SaaS)
# - API Gateway
# - AuthServer
# - Blazor WebApp

# Access Aspire Dashboard at: http://localhost:15888 (check console output for exact URL)
```

### Running Individual Services

```bash
# Run a single microservice (without Aspire)
cd services/administration/host/Tasky.Administration.HttpApi.Host
dotnet run

# Note: Requires manual infrastructure setup (PostgreSQL, Redis, RabbitMQ)
# Connection strings in appsettings.json must point to running infrastructure
```

### Testing

```bash
# Run all tests in solution
dotnet test Tasky.sln

# Run tests for specific service
dotnet test services/administration/test/Tasky.Administration.Application.Tests/Tasky.Administration.Application.Tests.csproj

# Run single test class
dotnet test --filter "FullyQualifiedName~SampleAppService_Tests"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Database Migrations

```bash
# Run migrations manually (normally handled by Aspire startup)
cd shared/Tasky.DbMigrator
dotnet run

# Add new migration to a service (example: Administration)
cd services/administration/src/Tasky.Administration.EntityFrameworkCore
dotnet ef migrations add MigrationName
```

### Database Reset (Development Only)

**Recommended: Using Aspire Dashboard**

When running the application with Aspire (`cd apps/Tasky.AppHost && dotnet run`):

1. Open the Aspire Dashboard (typically http://localhost:15888)
2. Navigate to the "Tasky-DbMigrator" resource
3. Click the **"Reset Databases"** button in the resource commands section
4. The command will drop and recreate all databases with fresh migrations

This custom command is configured in `apps/Tasky.AppHost/Program.cs` and provides visual feedback in the dashboard.

**Alternative: Manual Script Execution**

```bash
# On Windows (PowerShell)
.\reset-databases.ps1

# On Linux/macOS
chmod +x reset-databases.sh
./reset-databases.sh

# Manual reset (set environment variable directly)
cd shared/Tasky.DbMigrator
$env:RESET_DATABASES="true"  # PowerShell
# or
export RESET_DATABASES=true  # Bash
dotnet run
```

**What the reset does:**
- Drops all four databases (Administration, Identity, Projects, SaaS)
- Recreates databases with fresh schema from migrations
- Seeds permissions, features, and settings
- Creates default admin user: `admin` / `1q2w3E*`
- Seeds OpenIddict OAuth2 clients

**WARNING:** This permanently deletes ALL data in all databases!

### Code Formatting and Auto-Fixes

This project uses automated tools to format code and fix analyzer issues:

```bash
# Format entire solution with CSharpier
dotnet csharpier .

# Auto-fix analyzer diagnostics (Roslynator, SonarAnalyzer, etc.)
dotnet format analyzers src/Tasky.sln

# Auto-fix code style issues (IDE* rules)
dotnet format style src/Tasky.sln

# Run all auto-fixes and formatting
dotnet format analyzers src/Tasky.sln && dotnet format style src/Tasky.sln && dotnet csharpier .

# Check formatting without making changes
dotnet csharpier --check .
```

**Why CSharpier:**
- Opinionated (minimal configuration needed)
- Fast (10x faster than dotnet format)
- Consistent (no debates about style)
- Automatic via pre-commit hooks

**Why dotnet format:**
- Auto-fixes analyzer diagnostics (removes unused usings, simplifies expressions, etc.)
- Fixes code style violations (IDE* rules)
- Works with all Roslyn analyzers that provide code fixes

### Code Analysis

```bash
# Run all analyzers with strict enforcement
dotnet build /p:AnalysisMode=All /p:EnforceCodeStyleInBuild=true

# The solution includes comprehensive analyzers:
# - AsyncFixer (async/await best practices)
# - Microsoft.CodeAnalysis.NetAnalyzers (API usage, performance, security)
# - Microsoft.VisualStudio.Threading.Analyzers (threading safety)
# - StyleCop.Analyzers (code style consistency, ~300 rules)
# - SonarAnalyzer.CSharp (code quality, bugs, code smells)
# - Roslynator (C# best practices, 500+ analyzers)
# - SecurityCodeScan (security vulnerabilities: XSS, SQL injection, etc.)
# - ConfigureAwait.Fody (auto-configured via Fody IL weaving)

# View all analyzer diagnostics
dotnet build --verbosity normal | grep -E "warning|error"
```

### Git Hooks

The project uses Husky.Net for automatic code quality fixes on commit:

```bash
# Install/reinstall git hooks (run once after cloning)
dotnet tool restore
dotnet husky install

# Pre-commit hook automatically performs (in order):
# 1. Fix auto-fixable analyzer diagnostics (unused usings, simplify expressions, etc.)
# 2. Fix code style issues (IDE* rules)
# 3. Format code with CSharpier
# Only staged C# files are processed - fast and non-intrusive!

# Skip hooks if needed (use sparingly for emergency commits)
git commit --no-verify -m "message"
```

**What gets auto-fixed:**
- Unused using directives
- Unnecessary code (unused variables, redundant casts, etc.)
- Simplifiable expressions (use pattern matching, collection expressions, etc.)
- Code style violations (var usage, expression bodies, etc.)
- Formatting (indentation, spacing, line breaks)

## Architecture Overview

### Microservices Structure

The solution follows ABP's Domain-Driven Design (DDD) layered architecture. Each service is organized as:

```
services/{service-name}/
├── host/{Service}.HttpApi.Host/          # API host (entry point)
├── src/
│   ├── {Service}.Domain.Shared/          # Constants, enums (no dependencies)
│   ├── {Service}.Domain/                 # Entities, aggregates, domain services
│   ├── {Service}.Application.Contracts/  # DTOs, service interfaces
│   ├── {Service}.Application/            # Application services (business logic)
│   ├── {Service}.EntityFrameworkCore/    # EF Core DbContext, repositories
│   ├── {Service}.HttpApi/                # Controllers, API endpoints
│   └── {Service}.HttpApi.Client/         # Client proxies for remote calls
└── test/
    ├── {Service}.Domain.Tests/
    ├── {Service}.Application.Tests/
    ├── {Service}.EntityFrameworkCore.Tests/
    ├── {Service}.HttpApi.Client.ConsoleTestApp/
    └── {Service}.TestBase/
```

### Core Services

1. **Administration Service** (`services/administration/`) - Port 7001
   - System-wide administration, permissions, settings, audit logging, feature management
   - Database: `TaskyAdministrationDb`

2. **Identity Service** (`services/identity/`) - Port 7002
   - User/role management, ABP Identity integration
   - Shares AdministrationDbContext and SaaSDbContext for cross-cutting concerns
   - Database: `TaskyIdentityServiceDb`

3. **Projects Service** (`services/projects/`) - Port 7004
   - Business domain for project management
   - Database: `TaskyProjectsDb`

4. **SaaS Service** (`services/saas/`) - Port 7003
   - Multi-tenancy, tenant management, feature subscriptions
   - Database: `TaskySaaSDb`

### Infrastructure Components

**Gateway** (`gateway/Tasky.Gateway/`)
- YARP (Yet Another Reverse Proxy) for routing
- Path-based routing to backend services:
  - `/api/identity/*` → Identity Service
  - `/api/account/*` → Identity Service
  - `/api/multi-tenancy/*` → SaaS Service
  - `/api/feature-management/*` → SaaS Service
  - `/api/projects/*` → Projects Service
  - `{**catch-all}` → Administration Service
- JWT authentication enforcement
- Swagger/Scalar API documentation

**AuthServer** (`apps/Tasky.AuthServer/`) - Port 7600
- OpenIddict-based OAuth2/OIDC authorization server
- Handles token issuance for all services
- Connects to three databases (Administration, Identity, SaaS) for cross-cutting auth concerns
- OAuth2 flows: Authorization Code, Client Credentials, Resource Owner Password

**Shared Libraries** (`shared/`)
- `Tasky.ServiceDefaults`: ASP.NET Core hosting, OpenTelemetry, service discovery, resilience
- `Tasky.Hosting.Shared`: Redis caching, RabbitMQ integration, distributed locking, localization
- `Tasky.Microservice.Shared`: JWT auth config, CORS, Swagger setup for microservices
- `Tasky.Shared`: Centralized constants (`TaskyNames` for service/database names)
- `Tasky.DbMigrator`: Centralized migration tool for all service databases

### Key Architectural Patterns

**Module System:**
- Every component is an ABP module (`AbpModule`) with explicit `[DependsOn(...)]` declarations
- Modules are composed via dependency injection
- Example: `AdministrationHttpApiHostModule` depends on Application, Domain, EntityFrameworkCore modules

**Multi-Database Access:**
- Services own their databases but can access shared contexts:
  - Identity/AuthServer access Administration + SaaS contexts (for permissions, tenancy)
  - This is intentional for cross-cutting concerns, not a violation of service boundaries

**Event-Driven Communication:**
- RabbitMQ for async messaging via ABP's EventBus
- Services publish domain events that others subscribe to
- Exchange: "Tasky", per-service client names

**Distributed Infrastructure:**
- **Redis**: Distributed caching (per-service key prefixes), data protection keys, distributed locks (Medallion.Threading)
- **RabbitMQ**: Async events, background jobs
- **Seq**: Centralized logging via Serilog
- **OpenTelemetry**: Distributed tracing, metrics (ASP.NET Core, HTTP, Runtime instrumentation)

**Service Discovery:**
- Uses Microsoft.Extensions.ServiceDiscovery
- Services resolve each other by name (e.g., "TaskyAdministration")
- Aspire provides automatic name resolution in development

## ABP Framework Conventions

### Module Configuration

When creating or modifying ABP modules, follow these patterns:

```csharp
[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(YourDomainModule),
    typeof(YourApplicationModule),
    typeof(YourEntityFrameworkCoreModule)
)]
public class YourHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        // Configure services here
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // Configure middleware pipeline
        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI();
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
```

### Repository Pattern

ABP provides generic repositories. Use them instead of custom data access:

```csharp
// Inject repository in application service
private readonly IRepository<YourEntity, Guid> _repository;

// Common operations
var entity = await _repository.GetAsync(id);
await _repository.InsertAsync(entity);
await _repository.UpdateAsync(entity);
await _repository.DeleteAsync(id);

// Querying with LINQ
var query = await _repository.GetQueryableAsync();
var results = await AsyncExecuter.ToListAsync(query.Where(x => x.Name.Contains("test")));
```

### Application Services

Application services should inherit from `ApplicationService` and use DTOs:

```csharp
public class YourAppService : ApplicationService, IYourAppService
{
    private readonly IRepository<YourEntity, Guid> _repository;

    public YourAppService(IRepository<YourEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<YourDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<YourEntity, YourDto>(entity);
    }
}
```

### Domain Events

Publish domain events from aggregates, handle them in event handlers:

```csharp
// In domain service or entity
await LocalEventBus.PublishAsync(new YourDomainEvent { ... });

// Event handler
public class YourEventHandler : ILocalEventHandler<YourDomainEvent>
{
    public async Task HandleEventAsync(YourDomainEvent eventData)
    {
        // Handle event
    }
}
```

## Database Management

### Connection Strings

Connection strings are managed via .NET Aspire in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "TaskyAdministrationDb": "Server=localhost;Port=5432;Database=TaskyAdministrationDb;...",
    "TaskyIdentityServiceDb": "Server=localhost;Port=5432;Database=TaskyIdentityServiceDb;...",
    "TaskyProjectsDb": "Server=localhost;Port=5432;Database=TaskyProjectsDb;...",
    "TaskySaaSDb": "Server=localhost;Port=5432;Database=TaskySaaSDb;..."
  }
}
```

When running with Aspire, connection strings are injected automatically via service references.

### Migration Strategy

The solution uses **centralized database migrations** via `Tasky.DbMigrator`:

1. DbMigrator depends on all service EntityFrameworkCore modules
2. Runs migrations for all databases in sequence
3. Seeds OpenIddict clients/resources post-migration
4. Aspire startup orchestration:
   - Postgres container starts
   - DbMigrator runs and completes
   - All services wait for DbMigrator completion before starting

To add a migration to a service:

```bash
cd services/{service}/src/{Service}.EntityFrameworkCore
dotnet ef migrations add YourMigrationName

# Migration will be automatically applied by DbMigrator on next run
```

### Database Contexts

Services define their own `DbContext` inheriting from `AbpDbContext<T>`:

```csharp
public class YourServiceDbContext : AbpDbContext<YourServiceDbContext>
{
    public DbSet<YourEntity> YourEntities { get; set; }

    public YourServiceDbContext(DbContextOptions<YourServiceDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureYourService(); // Extension method with entity configs
    }
}
```

## Service Communication

### Inter-Service HTTP Calls

Use ABP's client proxies (HttpApi.Client projects) for type-safe service calls:

```csharp
// In a service that needs to call Identity service
// Add reference to Tasky.IdentityService.HttpApi.Client
// Inject the application service interface
private readonly IIdentityUserAppService _identityUserAppService;

public YourService(IIdentityUserAppService identityUserAppService)
{
    _identityUserAppService = identityUserAppService; // Auto-configured HTTP client
}

public async Task YourMethodAsync()
{
    var users = await _identityUserAppService.GetListAsync(new GetIdentityUsersInput());
}
```

The HttpApi.Client modules automatically configure HTTP clients with:
- Service discovery (resolves service name to URL)
- JWT token propagation
- Resilience policies (retry, circuit breaker, timeout)

### Distributed Events

For async, eventual consistency scenarios, use RabbitMQ-based distributed events:

```csharp
// Publish distributed event
await DistributedEventBus.PublishAsync(new YourEto { ... });

// Handle in another service
public class YourDistributedEventHandler : IDistributedEventHandler<YourEto>
{
    public async Task HandleEventAsync(YourEto eventData)
    {
        // Handle event from another service
    }
}
```

Distributed events are defined in `Application.Contracts` projects as DTOs (typically named `*Eto` for Event Transfer Object).

## Testing Patterns

### Application Layer Tests

```csharp
public class YourAppService_Tests : YourServiceApplicationTestBase
{
    private readonly IYourAppService _appService;

    public YourAppService_Tests()
    {
        _appService = GetRequiredService<IYourAppService>();
    }

    [Fact]
    public async Task Should_Get_Entity()
    {
        // Arrange
        var input = new GetEntityInput { Id = Guid.NewGuid() };

        // Act
        var result = await _appService.GetAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(input.Id);
    }
}
```

### Domain Tests

Focus on business logic without infrastructure:

```csharp
public class YourDomainService_Tests : YourServiceDomainTestBase
{
    private readonly IYourDomainService _domainService;

    public YourDomainService_Tests()
    {
        _domainService = GetRequiredService<IYourDomainService>();
    }

    [Fact]
    public async Task Should_Apply_Business_Rule()
    {
        // Test domain logic
    }
}
```

### Integration Tests (Console Test Apps)

Each service has a `HttpApi.Client.ConsoleTestApp` for manual integration testing:

```bash
cd services/{service}/test/{Service}.HttpApi.Client.ConsoleTestApp
dotnet run

# Manually verify HTTP API calls work end-to-end
```

## Common Development Scenarios

### Adding a New Entity to a Service

1. Define entity in `{Service}.Domain/Entities/YourEntity.cs`
2. Add to DbContext in `{Service}.EntityFrameworkCore/{Service}DbContext.cs`
3. Configure entity in `{Service}.EntityFrameworkCore/EntityConfigurations/YourEntityConfiguration.cs`
4. Create migration: `dotnet ef migrations add AddYourEntity`
5. Create DTO in `{Service}.Application.Contracts/YourEntity/YourEntityDto.cs`
6. Add object mapping in `{Service}.Application/{Service}ApplicationAutoMapperProfile.cs`
7. Create application service in `{Service}.Application/YourEntity/YourEntityAppService.cs`
8. Define controller in `{Service}.HttpApi/Controllers/YourEntityController.cs` (or use ABP conventions)

### Adding a New Microservice

Follow the existing service structure:

1. Create folder under `services/your-service/`
2. Copy structure from an existing service (e.g., `projects/`)
3. Rename namespaces and projects
4. Add service to `Tasky.sln`
5. Register in `Tasky.AppHost/Program.cs` with database and dependencies
6. Add service name constants to `Tasky.Shared/TaskyNames.cs`
7. Update gateway routing in `gateway/Tasky.Gateway/appsettings.json`
8. Add DbContext to `Tasky.DbMigrator`

### Configuring Authentication

Services use JWT Bearer tokens issued by AuthServer (port 7600):

```json
// In service appsettings.json
{
  "AuthServer": {
    "Authority": "https://localhost:7600",
    "RequireHttpsMetadata": "false",
    "SwaggerClientId": "YourService_Swagger",
    "SwaggerClientSecret": "1q2w3e*"
  }
}
```

OAuth2 clients are seeded in `Tasky.DbMigrator` via `OpenIddictDataSeeder.cs`.

### Working with Multi-Tenancy

Multi-tenancy is enabled via `Tasky.Shared/MultiTenancyConsts.cs`. Current value: `true`.

Tenant resolution happens via:
- `__tenant` header
- `{tenantName}.yourdomain.com` subdomain
- Query string `?__tenant={tenantId}`

Access current tenant in code:

```csharp
// Inject ICurrentTenant
private readonly ICurrentTenant _currentTenant;

var tenantId = _currentTenant.Id; // null for host
var isAvailable = _currentTenant.IsAvailable; // false if host context
```

### Logging and Observability

**Serilog Configuration:**
- Console output (async) in DEBUG mode
- File output in RELEASE mode (`Logs/` directory)
- Seq integration for centralized logs (http://localhost:5341 when running with Aspire)

**OpenTelemetry:**
- Traces: ASP.NET Core, HTTP client, custom activities
- Metrics: Request duration, HTTP client metrics, runtime counters
- Export to OTLP endpoint (configurable)

Access telemetry via Aspire Dashboard when running with `dotnet run` in AppHost.

## Important Files and Locations

- `Directory.Build.props` - Solution-wide package references and analyzers
- `Tasky.sln` - Main solution file
- `shared/Tasky.Shared/TaskyNames.cs` - Centralized service/database naming constants
- `apps/Tasky.AppHost/Program.cs` - Aspire orchestration and startup sequencing
- `gateway/Tasky.Gateway/appsettings.json` - YARP routing configuration
- `apps/Tasky.AuthServer/OpenIddictDataSeeder.cs` - OAuth2 client seeding
- `shared/Tasky.DbMigrator/` - Centralized database migration tool

## Troubleshooting

**Services fail to start:**
- Ensure Aspire is running infrastructure (Postgres, Redis, RabbitMQ)
- Check DbMigrator completed successfully
- Verify connection strings in appsettings.json

**Authentication errors:**
- Ensure AuthServer (port 7600) is running
- Check JWT token validity in Swagger
- Verify OAuth2 clients are seeded in database

**Database migration issues:**
- Run DbMigrator manually: `cd shared/Tasky.DbMigrator && dotnet run`
- Check PostgreSQL is accessible on port 5432
- Review migration logs in console output

**Service discovery failures:**
- Ensure services are running with Aspire orchestration
- Check service names match `TaskyNames` constants
- Verify Aspire Dashboard shows services as healthy
