# Running Integration Tests Guide

This guide provides detailed instructions on how to run integration tests for the microservices in this solution.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Setup](#initial-setup)
3. [Running Tests](#running-tests)
4. [Running Tests in Visual Studio](#running-tests-in-visual-studio)
5. [Running Tests in Visual Studio Code](#running-tests-in-visual-studio-code)
6. [Running Tests in CI/CD](#running-tests-in-cicd)
7. [Advanced Options](#advanced-options)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

Before running integration tests, ensure you have the following installed and configured:

### Required Software

1. **.NET 9.0 SDK or later**
   - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download)
   - Verify installation:
     ```bash
     dotnet --version
     ```
     Should output `9.0.x` or higher

2. **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
   - Download from [Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Docker must be running before executing tests
   - Verify Docker is running:
     ```bash
     docker --version
     docker ps
     ```

3. **Git** (if cloning from repository)
   - Verify installation:
     ```bash
     git --version
     ```

### System Requirements

- **Minimum 4GB RAM** available for Docker containers
- **Disk space**: At least 2GB free for Docker images and test databases
- **Network**: Internet connection for first-time Docker image downloads

## Initial Setup

### 1. Clone and Navigate to Repository

```bash
# If you haven't cloned the repository yet
git clone <repository-url>
cd AbpMicroservice
```

### 2. Restore NuGet Packages

Restore all NuGet packages for the solution:

```bash
# From the repository root
dotnet restore src/Tasky.sln
```

Or restore packages for a specific test project:

```bash
dotnet restore src/services/administration/test/Tasky.Administration.IntegrationTests
```

### 3. Verify Docker is Running

Ensure Docker Desktop (or Docker Engine) is running:

```bash
# Check Docker status
docker info

# Pull the PostgreSQL image if not already present
docker pull postgres:16-alpine
```

### 4. Build the Solution

Build the solution to ensure all projects compile:

```bash
# Build all projects
dotnet build src/Tasky.sln

# Or build a specific test project
dotnet build src/services/administration/test/Tasky.Administration.IntegrationTests
```

## Running Tests

### Running All Integration Tests

Run all integration tests across all services:

```bash
# From the repository root
dotnet test src/Tasky.sln --filter "FullyQualifiedName~IntegrationTests"

# Or navigate to the src directory
cd src
dotnet test Tasky.sln --filter "FullyQualifiedName~IntegrationTests"
```

### Running Tests for a Specific Service

Run integration tests for a specific microservice:

#### Administration Service

```bash
dotnet test src/services/administration/test/Tasky.Administration.IntegrationTests
```

#### Identity Service

```bash
dotnet test src/services/identity/test/Tasky.IdentityService.IntegrationTests
```

#### Projects Service

```bash
dotnet test src/services/projects/test/Tasky.Projects.IntegrationTests
```

#### SaaS Service

```bash
dotnet test src/services/saas/test/Tasky.SaaS.IntegrationTests
```

### Running a Specific Test Class

Run all tests in a specific test class:

```bash
# Using the class name
dotnet test --filter "FullyQualifiedName~ApiEndpointTests"

# Using the full namespace
dotnet test --filter "FullyQualifiedName~Tasky.Administration.Samples.ApiEndpointTests"
```

### Running a Specific Test Method

Run a single test method:

```bash
# Using the method name
dotnet test --filter "FullyQualifiedName~Should_Return_Ok_For_Sample_EndpointAsync"

# Using the full qualified name
dotnet test --filter "FullyQualifiedName~Tasky.Administration.Samples.ApiEndpointTests.Should_Return_Ok_For_Sample_EndpointAsync"
```

### Running Tests with Multiple Filters

Combine multiple filters:

```bash
# Run tests matching multiple criteria
dotnet test --filter "FullyQualifiedName~Administration&FullyQualifiedName~ApiEndpointTests"
```

## Running Tests in Visual Studio

### Using Test Explorer

1. **Open the Solution**
   - Open `src/Tasky.sln` in Visual Studio

2. **Build the Solution**
   - Press `Ctrl+Shift+B` or go to `Build > Build Solution`

3. **Open Test Explorer**
   - Go to `Test > Test Explorer` or press `Ctrl+E, T`

4. **Run All Tests**
   - Click "Run All" in Test Explorer
   - Or right-click the solution and select "Run Tests"

5. **Run Specific Tests**
   - Expand the test tree in Test Explorer
   - Right-click on a test class or method
   - Select "Run Selected Tests"

### Using Command Palette

1. Press `Ctrl+Q` to open Quick Actions
2. Type "Test: Run All Tests"
3. Select the command

### Test Output

- View test results in the Test Explorer window
- Check the "Output" window (View > Output) and select "Tests" from the dropdown
- View detailed logs in the Test Detail Summary

## Running Tests in Visual Studio Code

### Using the Test Explorer Extension

1. **Install Extensions**
   - Install "C# Dev Kit" extension
   - Install ".NET Extension Pack"

2. **Open the Solution**
   - Open the `src` folder in VS Code
   - Open `Tasky.sln`

3. **Run Tests**
   - Click the "Testing" icon in the sidebar (beaker icon)
   - Click the play button next to the test you want to run
   - Or use the command palette: `Ctrl+Shift+P` → "Test: Run All Tests"

### Using Integrated Terminal

Open the integrated terminal (`Ctrl+`` `) and use the same `dotnet test` commands as above.

## Running Tests in CI/CD

### GitHub Actions Example

```yaml
name: Integration Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      docker:
        image: docker:24-dind
        options: --privileged
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore src/Tasky.sln
    
    - name: Build
      run: dotnet build src/Tasky.sln --no-restore
    
    - name: Run integration tests
      run: dotnet test src/Tasky.sln --filter "FullyQualifiedName~IntegrationTests" --no-build --verbosity normal
```

### Azure DevOps Pipeline Example

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerComposeProjectName: 'abp-microservice'

stages:
- stage: Test
  displayName: 'Integration Tests'
  jobs:
  - job: IntegrationTests
    displayName: 'Run Integration Tests'
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '9.0.x'
    
    - task: DockerCompose@0
      inputs:
        action: 'Run services'
        dockerComposeFile: 'docker-compose.yml'
        detached: true
    
    - script: |
        dotnet restore src/Tasky.sln
        dotnet build src/Tasky.sln --no-restore
        dotnet test src/Tasky.sln --filter "FullyQualifiedName~IntegrationTests" --no-build --verbosity normal
      displayName: 'Run Tests'
```

## Advanced Options

### Verbose Output

Get detailed test output:

```bash
dotnet test --verbosity normal
dotnet test --verbosity detailed
dotnet test --verbosity diagnostic
```

### Parallel Execution

Control test parallelization:

```bash
# Run tests in parallel (default)
dotnet test --parallel

# Disable parallel execution
dotnet test --no-parallel

# Set maximum parallel workers
dotnet test --maxCpuCount 4
```

### Code Coverage

Generate code coverage reports:

```bash
# Install coverlet collector if not already present
dotnet add package coverlet.collector

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
```

### Logging

Configure test logging:

```bash
# Log to console
dotnet test --logger "console;verbosity=detailed"

# Log to file
dotnet test --logger "trx;LogFileName=test-results.trx"

# Log to both
dotnet test --logger "console;verbosity=detailed" --logger "trx;LogFileName=test-results.trx"
```

### Environment Variables

Set environment variables for tests:

```bash
# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Test"; dotnet test

# Windows CMD
set ASPNETCORE_ENVIRONMENT=Test && dotnet test

# Linux/Mac
ASPNETCORE_ENVIRONMENT=Test dotnet test
```

### Custom Test Settings

Create a `xunit.runner.json` file in your test project:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "methodDisplay": "classAndMethod",
  "diagnosticMessages": true
}
```

## Troubleshooting

### Common Issues and Solutions

#### 1. Docker Not Running

**Error**: `Docker.DotNet.DockerApiException: Docker API responded with status code=InternalServerError`

**Solution**:
- Ensure Docker Desktop is running
- Verify Docker is accessible: `docker ps`
- Restart Docker Desktop if needed

#### 2. Port Already in Use

**Error**: `Port 5432 is already in use`

**Solution**:
- Testcontainers automatically assigns random ports, but conflicts can occur
- Stop any running PostgreSQL instances: `docker ps` and `docker stop <container-id>`
- Check for other services using the port

#### 3. Test Container Startup Timeout

**Error**: `Container startup timeout`

**Solution**:
- Increase Docker resources (Memory/CPU) in Docker Desktop settings
- Check Docker logs: `docker logs <container-id>`
- Ensure sufficient disk space available

#### 4. Migration Failures

**Error**: `No constructor found for DbContext`

**Solution**:
- Verify the DbContext has a constructor accepting `DbContextOptions<T>`
- Ensure all migrations are included in the project
- Check connection string format

#### 5. Authentication Failures

**Error**: `Unauthorized` or `401` responses

**Solution**:
- Verify JWT token creation includes required claims
- Check `appsettings.Test.json` configuration
- Ensure test auth server URL matches configuration

#### 6. Build Errors

**Error**: `The type or namespace name 'X' could not be found`

**Solution**:
- Restore NuGet packages: `dotnet restore`
- Rebuild the solution: `dotnet build --no-incremental`
- Clean and rebuild: `dotnet clean && dotnet build`

#### 7. Slow Test Execution

**Solution**:
- Run tests in parallel (default behavior)
- Use test filters to run only relevant tests
- Ensure Docker has sufficient resources allocated
- Consider using a faster SSD for Docker volumes

### Debugging Tips

1. **Run Tests with Debugger**
   ```bash
   # Attach debugger before running tests
   dotnet test --no-build --logger "console;verbosity=detailed"
   ```

2. **Check Container Logs**
   ```bash
   # List running containers
   docker ps
   
   # View container logs
   docker logs <container-id>
   ```

3. **Inspect Test Database**
   ```bash
   # Connect to test database container
   docker exec -it <container-id> psql -U postgres -d test_administration_db
   ```

4. **Enable Diagnostic Logging**
   ```bash
   dotnet test --verbosity diagnostic --logger "console;verbosity=detailed"
   ```

### Getting Help

If you encounter issues not covered here:

1. Check the main [Integration Tests Documentation](integration-tests.md)
2. Review test project README files
3. Check GitHub Issues for similar problems
4. Review Docker and .NET test documentation

## Best Practices

1. **Run Tests Locally Before Committing**
   - Run affected service tests before committing changes
   - Ensure all tests pass locally

2. **Use Test Filters**
   - Use filters to run only relevant tests during development
   - Save time by not running all tests for small changes

3. **Keep Docker Resources Adequate**
   - Allocate at least 4GB RAM to Docker
   - Ensure sufficient CPU cores for parallel execution

4. **Monitor Test Execution Time**
   - Track test execution time
   - Optimize slow tests if they exceed acceptable thresholds

5. **Clean Up After Tests**
   - Testcontainers automatically clean up, but verify
   - Manually clean up if needed: `docker system prune`

## Additional Resources

- [.NET Test Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [Docker Documentation](https://docs.docker.com/)

## Quick Reference

### Most Common Commands

```bash
# Run all integration tests
dotnet test src/Tasky.sln --filter "FullyQualifiedName~IntegrationTests"

# Run specific service tests
dotnet test src/services/administration/test/Tasky.Administration.IntegrationTests

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~ApiEndpointTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~Should_Return_Ok_For_Sample_EndpointAsync"
```

### Test Project Paths

- **Administration**: `src/services/administration/test/Tasky.Administration.IntegrationTests`
- **Identity**: `src/services/identity/test/Tasky.IdentityService.IntegrationTests`
- **Projects**: `src/services/projects/test/Tasky.Projects.IntegrationTests`
- **SaaS**: `src/services/saas/test/Tasky.SaaS.IntegrationTests`
