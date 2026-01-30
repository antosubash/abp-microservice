# ABP Microservice Makefile
# Common development tasks for the Tasky microservices project

.PHONY: help install build test clean format fix run restore hooks

# Default target - show help
help:
	@echo "Available targets:"
	@echo "  make install       - Restore .NET tools and install git hooks"
	@echo "  make restore       - Restore NuGet packages and .NET tools"
	@echo "  make hooks         - Install git hooks"
	@echo "  make build         - Build the entire solution"
	@echo "  make test          - Run all tests"
	@echo "  make clean         - Clean build artifacts"
	@echo "  make format        - Format code with CSharpier"
	@echo "  make fix           - Auto-fix analyzers and style issues"
	@echo "  make fix-all       - Run complete fix pipeline (analyzers + style + format)"
	@echo "  make check-format  - Check formatting without making changes"
	@echo "  make run           - Run application with .NET Aspire orchestration"
	@echo "  make run-gateway   - Run API Gateway only"
	@echo "  make run-auth      - Run AuthServer only"
	@echo "  make reset-db      - Reset all databases (DESTRUCTIVE - dev only)"
	@echo "  make migrate       - Run database migrations"
	@echo ""

# Install tools and hooks (first-time setup)
install:
	@echo "Installing .NET tools and git hooks..."
	dotnet tool restore
	dotnet husky install
	@echo "Setup complete!"

# Restore NuGet packages and tools
restore:
	@echo "Restoring NuGet packages and .NET tools..."
	dotnet tool restore
	cd src && dotnet restore Tasky.sln
	@echo "Restore complete!"

# Install git hooks only
hooks:
	@echo "Installing git hooks..."
	dotnet husky install
	@echo "Hooks installed!"

# Build the solution
build:
	@echo "Building solution..."
	cd src && dotnet build Tasky.sln
	@echo "Build complete!"

# Build with strict analyzer enforcement
build-strict:
	@echo "Building with strict analyzer enforcement..."
	cd src && dotnet build Tasky.sln /p:TreatWarningsAsErrors=true
	@echo "Build complete!"

# Run all tests
test:
	@echo "Running tests..."
	cd src && dotnet test Tasky.sln
	@echo "Tests complete!"

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	cd src && dotnet clean Tasky.sln
	@echo "Clean complete!"

# Format code with CSharpier
format:
	@echo "Formatting code with CSharpier..."
	dotnet csharpier src/
	@echo "Formatting complete!"

# Check formatting without making changes
check-format:
	@echo "Checking code formatting..."
	dotnet csharpier --check src/
	@echo "Format check complete!"

# Auto-fix analyzer diagnostics
fix-analyzers:
	@echo "Auto-fixing analyzer diagnostics..."
	cd src && dotnet format analyzers Tasky.sln
	@echo "Analyzer fixes complete!"

# Auto-fix code style issues
fix-style:
	@echo "Auto-fixing code style issues..."
	cd src && dotnet format style Tasky.sln
	@echo "Style fixes complete!"

# Run complete fix pipeline (analyzers + style + format)
fix: fix-analyzers fix-style format
	@echo "All fixes applied!"

# Alias for fix
fix-all: fix

# Run application with Aspire orchestration
run:
	@echo "Starting application with .NET Aspire..."
	@echo "Aspire Dashboard will be available at http://localhost:15888"
	cd src/apps/Tasky.AppHost && dotnet run

# Run API Gateway only
run-gateway:
	@echo "Starting API Gateway..."
	cd src/gateway/Tasky.Gateway && dotnet run

# Run AuthServer only
run-auth:
	@echo "Starting AuthServer..."
	cd src/apps/Tasky.AuthServer && dotnet run

# Run Administration service only
run-admin:
	@echo "Starting Administration Service..."
	cd src/services/administration/host/Tasky.Administration.HttpApi.Host && dotnet run

# Run Identity service only
run-identity:
	@echo "Starting Identity Service..."
	cd src/services/identity/host/Tasky.IdentityService.HttpApi.Host && dotnet run

# Run Projects service only
run-projects:
	@echo "Starting Projects Service..."
	cd src/services/projects/host/Tasky.Projects.HttpApi.Host && dotnet run

# Run SaaS service only
run-saas:
	@echo "Starting SaaS Service..."
	cd src/services/saas/host/Tasky.SaaS.HttpApi.Host && dotnet run

# Run database migrations
migrate:
	@echo "Running database migrations..."
	cd src/shared/Tasky.DbMigrator && dotnet run
	@echo "Migrations complete!"

# Reset databases (DESTRUCTIVE - development only)
reset-db:
	@echo "WARNING: This will DELETE ALL DATA in all databases!"
	@echo "Press Ctrl+C to cancel, or Enter to continue..."
	@read -p ""
	@echo "Resetting databases..."
	cd src && powershell -ExecutionPolicy Bypass -File ../reset-databases.ps1 || bash ../reset-databases.sh
	@echo "Databases reset complete!"

# View build warnings and errors
warnings:
	@echo "Building and showing warnings/errors..."
	cd src && dotnet build Tasky.sln --verbosity normal 2>&1 | grep -E "warning|error" || echo "No warnings or errors found!"

# Count lines of code (requires cloc tool)
loc:
	@echo "Counting lines of code..."
	@command -v cloc >/dev/null 2>&1 || { echo "cloc not installed. Install with: npm install -g cloc"; exit 1; }
	cloc src/ --exclude-dir=bin,obj,Logs,Migrations,node_modules --exclude-ext=json,xml

# Show project structure
tree:
	@echo "Project structure:"
	@command -v tree >/dev/null 2>&1 && tree -L 3 -I 'bin|obj|Logs|node_modules' src/ || ls -R src/

# Quick status check
status:
	@echo "=== Git Status ==="
	git status -s
	@echo ""
	@echo "=== .NET Version ==="
	dotnet --version
	@echo ""
	@echo "=== Installed Tools ==="
	dotnet tool list
	@echo ""

# Watch and rebuild on changes
watch:
	@echo "Watching for changes and rebuilding..."
	cd src && dotnet watch --project apps/Tasky.AppHost/Tasky.AppHost.csproj run

# Generate code coverage report (requires coverlet)
coverage:
	@echo "Running tests with coverage..."
	cd src && dotnet test Tasky.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
	@echo "Coverage report generated!"

# Update all NuGet packages
update-packages:
	@echo "Updating NuGet packages..."
	cd src && dotnet list package --outdated
	@echo "To update a specific package, run: dotnet add package <PackageName>"

# Check for security vulnerabilities in dependencies
security-check:
	@echo "Checking for security vulnerabilities..."
	cd src && dotnet list package --vulnerable --include-transitive
	@echo "Security check complete!"

# Create a new migration for a service
# Usage: make migration SERVICE=administration NAME=AddNewTable
migration:
	@if [ -z "$(SERVICE)" ] || [ -z "$(NAME)" ]; then \
		echo "Usage: make migration SERVICE=administration NAME=AddNewTable"; \
		exit 1; \
	fi
	@echo "Creating migration $(NAME) for $(SERVICE) service..."
	cd src/services/$(SERVICE)/src/Tasky.$(shell echo $(SERVICE) | sed 's/.*/\u&/').EntityFrameworkCore && \
	dotnet ef migrations add $(NAME)
	@echo "Migration created! Run 'make migrate' to apply."

# Show analyzer diagnostics
analyzers:
	@echo "Building and showing analyzer diagnostics..."
	cd src && dotnet build Tasky.sln /p:AnalysisMode=All /p:EnforceCodeStyleInBuild=true --verbosity normal 2>&1 | \
	grep -E "warning (CA|SA|RCS|S|SCS|AsyncFixer|VSTHRD)" | head -50 || echo "No analyzer warnings found!"

# Help for developers
dev-help:
	@echo "=== Quick Start Guide ==="
	@echo ""
	@echo "First time setup:"
	@echo "  1. make install          - Install tools and hooks"
	@echo "  2. make build            - Build the solution"
	@echo "  3. make run              - Run with Aspire"
	@echo ""
	@echo "Daily workflow:"
	@echo "  make format              - Format your code"
	@echo "  make fix                 - Auto-fix all issues"
	@echo "  make build               - Build and check for errors"
	@echo "  make test                - Run tests"
	@echo ""
	@echo "Git commits:"
	@echo "  - Pre-commit hook automatically runs fix + format"
	@echo "  - Bypass with: git commit --no-verify (emergency only)"
	@echo ""
	@echo "Database operations:"
	@echo "  make migrate             - Run migrations"
	@echo "  make reset-db            - Reset databases (dev only)"
	@echo ""
	@echo "For more targets, run: make help"
	@echo ""
