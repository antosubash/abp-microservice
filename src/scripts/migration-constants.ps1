# Migration Constants
# Service mappings for Entity Framework Core migrations

$script:Services = @{
    "administration" = @{
        Name = "administration"
        DbContext = "AdministrationDbContext"
        ProjectPath = "src\services\administration\src\Tasky.Administration.EntityFrameworkCore"
        ProjectName = "Tasky.Administration.EntityFrameworkCore"
        HostProjectPath = "src\services\administration\host\Tasky.Administration.HttpApi.Host"
        HostProjectName = "Tasky.Administration.HttpApi.Host"
        MigrationsFolder = "Migrations"
    }
    "identity" = @{
        Name = "identity"
        DbContext = "IdentityServiceDbContext"
        ProjectPath = "src\services\identity\src\Tasky.IdentityService.EntityFrameworkCore"
        ProjectName = "Tasky.IdentityService.EntityFrameworkCore"
        HostProjectPath = "src\services\identity\host\Tasky.IdentityService.HttpApi.Host"
        HostProjectName = "Tasky.IdentityService.HttpApi.Host"
        MigrationsFolder = "Migrations"
    }
    "projects" = @{
        Name = "projects"
        DbContext = "ProjectsDbContext"
        ProjectPath = "src\services\projects\src\Tasky.Projects.EntityFrameworkCore"
        ProjectName = "Tasky.Projects.EntityFrameworkCore"
        HostProjectPath = "src\services\projects\host\Tasky.Projects.HttpApi.Host"
        HostProjectName = "Tasky.Projects.HttpApi.Host"
        MigrationsFolder = "Migrations"
    }
    "saas" = @{
        Name = "saas"
        DbContext = "SaaSDbContext"
        ProjectPath = "src\services\saas\src\Tasky.SaaS.EntityFrameworkCore"
        ProjectName = "Tasky.SaaS.EntityFrameworkCore"
        HostProjectPath = "src\services\saas\host\Tasky.SaaS.HttpApi.Host"
        HostProjectName = "Tasky.SaaS.HttpApi.Host"
        MigrationsFolder = "Migrations"
    }
}

function Get-ServiceInfo {
    param(
        [string]$ServiceName
    )
    
    $serviceKey = $ServiceName.ToLower()
    if ($script:Services.ContainsKey($serviceKey)) {
        return $script:Services[$serviceKey]
    }
    return $null
}

function Get-AllServices {
    return $script:Services.Values
}

function Test-ServiceName {
    param(
        [string]$ServiceName
    )
    
    $serviceKey = $ServiceName.ToLower()
    return $script:Services.ContainsKey($serviceKey)
}

