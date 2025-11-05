#Requires -Version 5.1

<#
.SYNOPSIS
    Resets Entity Framework Core migrations for one or all services.

.DESCRIPTION
    Deletes all existing migrations and creates a new "Init" migration.
    By default, processes all services (administration, identity, projects, saas).
    Can target a specific service using the -Service parameter.

.PARAMETER Service
    Optional service name (administration, identity, projects, saas).
    If omitted, processes all services.

.PARAMETER Force
    Skip confirmation prompt.

.EXAMPLE
    .\reset-migration.ps1
    Resets migrations for all services.

.EXAMPLE
    .\reset-migration.ps1 -Service administration
    Resets migrations for the administration service only.

.EXAMPLE
    .\reset-migration.ps1 -Force
    Resets migrations for all services without confirmation.
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Service,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Load constants
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
. "$scriptPath\migration-constants.ps1"

# Determine root directory (assuming script is in src/scripts)
$rootDir = Split-Path -Parent (Split-Path -Parent $scriptPath)
Push-Location $rootDir

function Reset-MigrationForService {
    param(
        [hashtable]$ServiceInfo
    )
    
    $serviceName = $ServiceInfo.Name
    $dbContext = $ServiceInfo.DbContext
    $projectPath = $ServiceInfo.ProjectPath
    $projectName = $ServiceInfo.ProjectName
    $hostProjectPath = $ServiceInfo.HostProjectPath
    $hostProjectName = $ServiceInfo.HostProjectName
    $migrationsFolder = $ServiceInfo.MigrationsFolder
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Processing service: $serviceName" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $fullProjectPath = Join-Path $rootDir $projectPath
    $fullMigrationsPath = Join-Path $fullProjectPath $migrationsFolder
    
    if (-not (Test-Path $fullProjectPath)) {
        Write-Warning "Project path does not exist: $fullProjectPath"
        return $false
    }
    
    if (-not (Test-Path $fullMigrationsPath)) {
        Write-Warning "Migrations folder does not exist: $fullMigrationsPath"
        return $false
    }
    
    # Find all migration files
    $migrationFiles = Get-ChildItem -Path $fullMigrationsPath -Filter "*.cs" -File | Where-Object {
        $_.Name -match '^\d{14}_' -or $_.Name -match 'ModelSnapshot\.cs$'
    }
    
    if ($migrationFiles.Count -eq 0) {
        Write-Host "No migrations found for $serviceName" -ForegroundColor Yellow
    } else {
        Write-Host "`nMigrations to be deleted:" -ForegroundColor Yellow
        foreach ($file in $migrationFiles) {
            Write-Host "  - $($file.Name)" -ForegroundColor Gray
        }
        
        # Delete migration files
        Write-Host "`nDeleting migration files..." -ForegroundColor Yellow
        foreach ($file in $migrationFiles) {
            try {
                Remove-Item -Path $file.FullName -Force
                Write-Host "  Deleted: $($file.Name)" -ForegroundColor Green
            } catch {
                Write-Error "Failed to delete $($file.Name): $_"
                return $false
            }
        }
        Write-Host "Migration files deleted successfully." -ForegroundColor Green
    }
    
    # Add new Init migration
    Write-Host "`nAdding new Init migration..." -ForegroundColor Yellow
    $hostProjectFile = Join-Path $rootDir (Join-Path $hostProjectPath "$hostProjectName.csproj")
    $efProjectFile = Join-Path $fullProjectPath "$projectName.csproj"
    
    if (-not (Test-Path $efProjectFile)) {
        Write-Error "Project file not found: $efProjectFile"
        return $false
    }
    
    if (-not (Test-Path $hostProjectFile)) {
        Write-Warning "Host project file not found: $hostProjectFile. Using EntityFrameworkCore project only."
        $hostProjectFile = $null
    }
    
    try {
        if ($hostProjectFile) {
            $result = dotnet ef migrations add Init `
                --project $efProjectFile `
                --startup-project $hostProjectFile `
                --context $dbContext `
                2>&1
        } else {
            $result = dotnet ef migrations add Init `
                --project $efProjectFile `
                --context $dbContext `
                2>&1
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Init migration added successfully for $serviceName" -ForegroundColor Green
            return $true
        } else {
            Write-Error "Failed to add Init migration for $serviceName"
            Write-Host $result -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Error "Error adding Init migration for $serviceName : $($_.Exception.Message)"
        return $false
    }
}

# Main execution
try {
    $servicesToProcess = @()
    
    if ($Service) {
        if (-not (Test-ServiceName $Service)) {
            Write-Error "Invalid service name: $Service. Valid services are: administration, identity, projects, saas"
            exit 1
        }
        $serviceInfo = Get-ServiceInfo $Service
        $servicesToProcess = @($serviceInfo)
    } else {
        Write-Host "No service specified. Processing all services." -ForegroundColor Cyan
        $allServices = Get-AllServices
        $servicesToProcess = $allServices
    }
    
    # Show what will be processed
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Migration Reset Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Services to process: $($servicesToProcess.Count)" -ForegroundColor White
    foreach ($svc in $servicesToProcess) {
        Write-Host "  - $($svc.Name)" -ForegroundColor Gray
    }
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    # Confirmation
    if (-not $Force) {
        $confirmation = Read-Host "This will delete all existing migrations and create new Init migrations. Continue? (y/N)"
        if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
            Write-Host "Operation cancelled." -ForegroundColor Yellow
            exit 0
        }
    }
    
    # Process each service
    $successCount = 0
    $failCount = 0
    
    foreach ($serviceInfo in $servicesToProcess) {
        if (Reset-MigrationForService -ServiceInfo $serviceInfo) {
            $successCount++
        } else {
            $failCount++
        }
    }
    
    # Summary
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Migration Reset Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Success: $successCount" -ForegroundColor Green
    Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    if ($failCount -gt 0) {
        exit 1
    }
    
} catch {
    Write-Error "An error occurred: $_"
    exit 1
} finally {
    Pop-Location
}

