#!/bin/bash
# Migration Constants
# Service mappings for Entity Framework Core migrations

# Service definitions
declare -A SERVICES

# Administration service
SERVICES[administration,name]="administration"
SERVICES[administration,dbcontext]="AdministrationDbContext"
SERVICES[administration,projectpath]="src/services/administration/src/Tasky.Administration.EntityFrameworkCore"
SERVICES[administration,projectname]="Tasky.Administration.EntityFrameworkCore"
SERVICES[administration,hostprojectpath]="src/services/administration/host/Tasky.Administration.HttpApi.Host"
SERVICES[administration,hostprojectname]="Tasky.Administration.HttpApi.Host"
SERVICES[administration,migrationsfolder]="Migrations"

# Identity service
SERVICES[identity,name]="identity"
SERVICES[identity,dbcontext]="IdentityServiceDbContext"
SERVICES[identity,projectpath]="src/services/identity/src/Tasky.IdentityService.EntityFrameworkCore"
SERVICES[identity,projectname]="Tasky.IdentityService.EntityFrameworkCore"
SERVICES[identity,hostprojectpath]="src/services/identity/host/Tasky.IdentityService.HttpApi.Host"
SERVICES[identity,hostprojectname]="Tasky.IdentityService.HttpApi.Host"
SERVICES[identity,migrationsfolder]="Migrations"

# Projects service
SERVICES[projects,name]="projects"
SERVICES[projects,dbcontext]="ProjectsDbContext"
SERVICES[projects,projectpath]="src/services/projects/src/Tasky.Projects.EntityFrameworkCore"
SERVICES[projects,projectname]="Tasky.Projects.EntityFrameworkCore"
SERVICES[projects,hostprojectpath]="src/services/projects/host/Tasky.Projects.HttpApi.Host"
SERVICES[projects,hostprojectname]="Tasky.Projects.HttpApi.Host"
SERVICES[projects,migrationsfolder]="Migrations"

# SaaS service
SERVICES[saas,name]="saas"
SERVICES[saas,dbcontext]="SaaSDbContext"
SERVICES[saas,projectpath]="src/services/saas/src/Tasky.SaaS.EntityFrameworkCore"
SERVICES[saas,projectname]="Tasky.SaaS.EntityFrameworkCore"
SERVICES[saas,hostprojectpath]="src/services/saas/host/Tasky.SaaS.HttpApi.Host"
SERVICES[saas,hostprojectname]="Tasky.SaaS.HttpApi.Host"
SERVICES[saas,migrationsfolder]="Migrations"

# Get service info
get_service_info() {
    local service_name=$1
    local service_key=$(echo "$service_name" | tr '[:upper:]' '[:lower:]')
    
    if [[ -n "${SERVICES[$service_key,name]}" ]]; then
        echo "$service_key"
    else
        echo ""
    fi
}

# Get all services
get_all_services() {
    echo "administration identity projects saas"
}

# Test service name
test_service_name() {
    local service_name=$1
    local service_key=$(echo "$service_name" | tr '[:upper:]' '[:lower:]')
    
    if [[ -n "${SERVICES[$service_key,name]}" ]]; then
        return 0
    else
        return 1
    fi
}

# Get service property
get_service_property() {
    local service_key=$1
    local property=$2
    echo "${SERVICES[$service_key,$property]}"
}

