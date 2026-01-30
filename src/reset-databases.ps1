#!/usr/bin/env pwsh
# Reset Databases - Drops and recreates all databases with fresh migrations

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Tasky Database Reset Utility" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "WARNING: This will DROP all databases and recreate them!" -ForegroundColor Yellow
Write-Host "All existing data will be PERMANENTLY LOST." -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Resetting databases..." -ForegroundColor Cyan

# Set environment variable and run DbMigrator
$env:RESET_DATABASES = "true"

Push-Location "shared\Tasky.DbMigrator"
try {
    dotnet run --configuration Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "[SUCCESS] Databases reset successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "All databases have been dropped and recreated with:" -ForegroundColor Green
        Write-Host "  - Fresh schema migrations" -ForegroundColor Green
        Write-Host "  - Seeded permissions, features, and settings" -ForegroundColor Green
        Write-Host "  - Default admin user (admin / 1q2w3E*)" -ForegroundColor Green
        Write-Host "  - OpenIddict clients" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "[ERROR] Database reset failed!" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
    Remove-Item Env:\RESET_DATABASES
}
