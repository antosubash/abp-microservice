#!/bin/bash

# Reset Entity Framework Core migrations for one or all services.
# Deletes all existing migrations and creates a new "Init" migration.
# By default, processes all services (administration, identity, projects, saas).
# Can target a specific service using the -s parameter.

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

# Load constants
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/migration-constants.sh"

# Determine root directory (assuming script is in src/scripts)
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Parse arguments
SERVICE=""
FORCE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -s|--service)
            SERVICE="$2"
            shift 2
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [-s SERVICE] [-f]"
            echo ""
            echo "Options:"
            echo "  -s, --service SERVICE  Service name (administration, identity, projects, saas)"
            echo "                         If omitted, processes all services"
            echo "  -f, --force           Skip confirmation prompt"
            echo "  -h, --help            Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Function to reset migrations for a service
reset_migration_for_service() {
    local service_key=$1
    local service_name=$(get_service_property "$service_key" "name")
    local db_context=$(get_service_property "$service_key" "dbcontext")
    local project_path=$(get_service_property "$service_key" "projectpath")
    local project_name=$(get_service_property "$service_key" "projectname")
    local host_project_path=$(get_service_property "$service_key" "hostprojectpath")
    local host_project_name=$(get_service_property "$service_key" "hostprojectname")
    local migrations_folder=$(get_service_property "$service_key" "migrationsfolder")
    
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}Processing service: $service_name${NC}"
    echo -e "${CYAN}========================================${NC}"
    
    local full_project_path="$ROOT_DIR/$project_path"
    local full_migrations_path="$full_project_path/$migrations_folder"
    
    if [[ ! -d "$full_project_path" ]]; then
        echo -e "${YELLOW}Warning: Project path does not exist: $full_project_path${NC}"
        return 1
    fi
    
    if [[ ! -d "$full_migrations_path" ]]; then
        echo -e "${YELLOW}Warning: Migrations folder does not exist: $full_migrations_path${NC}"
        return 1
    fi
    
    # Find all migration files
    local migration_files=()
    while IFS= read -r -d '' file; do
        migration_files+=("$file")
    done < <(find "$full_migrations_path" -maxdepth 1 -type f -name "*.cs" \( -name "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]_*.cs" -o -name "*ModelSnapshot.cs" \) -print0 2>/dev/null || true)
    
    if [[ ${#migration_files[@]} -eq 0 ]]; then
        echo -e "${YELLOW}No migrations found for $service_name${NC}"
    else
        echo ""
        echo -e "${YELLOW}Migrations to be deleted:${NC}"
        for file in "${migration_files[@]}"; do
            echo -e "${GRAY}  - $(basename "$file")${NC}"
        done
        
        # Delete migration files
        echo ""
        echo -e "${YELLOW}Deleting migration files...${NC}"
        for file in "${migration_files[@]}"; do
            if rm -f "$file"; then
                echo -e "${GREEN}  Deleted: $(basename "$file")${NC}"
            else
                echo -e "${RED}Failed to delete: $(basename "$file")${NC}"
                return 1
            fi
        done
        echo -e "${GREEN}Migration files deleted successfully.${NC}"
    fi
    
    # Add new Init migration
    echo ""
    echo -e "${YELLOW}Adding new Init migration...${NC}"
    local host_project_file="$ROOT_DIR/$host_project_path/$host_project_name.csproj"
    local ef_project_file="$full_project_path/$project_name.csproj"
    
    if [[ ! -f "$ef_project_file" ]]; then
        echo -e "${RED}Error: Project file not found: $ef_project_file${NC}"
        return 1
    fi
    
    if [[ ! -f "$host_project_file" ]]; then
        echo -e "${YELLOW}Warning: Host project file not found: $host_project_file. Using EntityFrameworkCore project only.${NC}"
        if dotnet ef migrations add Init \
            --project "$ef_project_file" \
            --context "$db_context" 2>&1; then
            echo -e "${GREEN}Init migration added successfully for $service_name${NC}"
            return 0
        else
            echo -e "${RED}Failed to add Init migration for $service_name${NC}"
            return 1
        fi
    else
        if dotnet ef migrations add Init \
            --project "$ef_project_file" \
            --startup-project "$host_project_file" \
            --context "$db_context" 2>&1; then
            echo -e "${GREEN}Init migration added successfully for $service_name${NC}"
            return 0
        else
            echo -e "${RED}Failed to add Init migration for $service_name${NC}"
            return 1
        fi
    fi
}

# Main execution
cd "$ROOT_DIR"

SERVICES_TO_PROCESS=()

if [[ -n "$SERVICE" ]]; then
    service_key=$(echo "$SERVICE" | tr '[:upper:]' '[:lower:]')
    if ! test_service_name "$SERVICE"; then
        echo -e "${RED}Error: Invalid service name: $SERVICE${NC}"
        echo "Valid services are: administration, identity, projects, saas"
        exit 1
    fi
    SERVICES_TO_PROCESS=("$service_key")
else
    echo -e "${CYAN}No service specified. Processing all services.${NC}"
    SERVICES_TO_PROCESS=($(get_all_services))
fi

# Show what will be processed
echo ""
echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}Migration Reset Summary${NC}"
echo -e "${CYAN}========================================${NC}"
echo -e "${NC}Services to process: ${#SERVICES_TO_PROCESS[@]}"
for service_key in "${SERVICES_TO_PROCESS[@]}"; do
    service_name=$(get_service_property "$service_key" "name")
    echo -e "${GRAY}  - $service_name${NC}"
done
echo -e "${CYAN}========================================${NC}"
echo ""

# Confirmation
if [[ "$FORCE" != "true" ]]; then
    read -p "This will delete all existing migrations and create new Init migrations. Continue? (y/N): " confirmation
    if [[ ! "$confirmation" =~ ^[Yy]$ ]]; then
        echo -e "${YELLOW}Operation cancelled.${NC}"
        exit 0
    fi
fi

# Process each service
SUCCESS_COUNT=0
FAIL_COUNT=0

for service_key in "${SERVICES_TO_PROCESS[@]}"; do
    if reset_migration_for_service "$service_key"; then
        ((SUCCESS_COUNT++))
    else
        ((FAIL_COUNT++))
    fi
done

# Summary
echo ""
echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}Migration Reset Summary${NC}"
echo -e "${CYAN}========================================${NC}"
echo -e "${GREEN}Success: $SUCCESS_COUNT${NC}"
if [[ $FAIL_COUNT -gt 0 ]]; then
    echo -e "${RED}Failed: $FAIL_COUNT${NC}"
else
    echo -e "${GREEN}Failed: $FAIL_COUNT${NC}"
fi
echo -e "${CYAN}========================================${NC}"
echo ""

if [[ $FAIL_COUNT -gt 0 ]]; then
    exit 1
fi

exit 0

