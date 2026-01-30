#!/bin/bash
# Reset Databases - Drops and recreates all databases with fresh migrations

echo "=================================================="
echo "  Tasky Database Reset Utility"
echo "=================================================="
echo ""
echo "WARNING: This will DROP all databases and recreate them!"
echo "All existing data will be PERMANENTLY LOST."
echo ""

read -p "Are you sure you want to continue? (yes/no): " confirmation
if [ "$confirmation" != "yes" ]; then
    echo "Operation cancelled."
    exit 0
fi

echo ""
echo "Resetting databases..."

# Set environment variable and run DbMigrator
export RESET_DATABASES=true

cd "shared/Tasky.DbMigrator" || exit 1

dotnet run --configuration Release
exit_code=$?

if [ $exit_code -eq 0 ]; then
    echo ""
    echo "✓ Databases reset successfully!"
    echo ""
    echo "All databases have been dropped and recreated with:"
    echo "  • Fresh schema migrations"
    echo "  • Seeded permissions, features, and settings"
    echo "  • Default admin user (admin / 1q2w3E*)"
    echo "  • OpenIddict clients"
else
    echo ""
    echo "✗ Database reset failed!"
    exit 1
fi

cd ../..
unset RESET_DATABASES
