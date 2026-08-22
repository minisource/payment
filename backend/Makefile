.PHONY: build test clean publish restore run

# .NET Solution Management for Payment Service

# Build the solution
build:
	dotnet build payment.slnx

# Build in Release mode
build-release:
	dotnet build payment.slnx -c Release

# Run tests
test:
	dotnet test payment.slnx

# Run tests with coverage
test-coverage:
	dotnet test payment.slnx --collect:"XPlat Code Coverage"

# Clean build artifacts
clean:
	dotnet clean payment.slnx
	rm -rf Application/bin Application/obj
	rm -rf Domain/bin Domain/obj
	rm -rf Infrastructure/bin Infrastructure/obj
	rm -rf payment/bin payment/obj

# Restore packages
restore:
	dotnet restore payment.slnx

# Run the application
run:
	dotnet run --project payment

# Run in development mode
run-dev:
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project payment

# Watch mode for development
watch:
	dotnet watch --project payment run

# Publish the application
publish:
	dotnet publish payment.slnx -c Release -o ./publish

# Database migrations (EF Core)
migration-add:
	@read -p "Migration name: " name; \
	dotnet ef migrations add $$name --project Infrastructure --startup-project payment

migration-update:
	dotnet ef database update --project Infrastructure --startup-project payment

migration-remove:
	dotnet ef migrations remove --project Infrastructure --startup-project payment

# Docker commands
docker-build:
	docker build -t minisource/payment:latest .

docker-run:
	docker run -p 4005:4005 --env-file .env minisource/payment:latest

docker-up:
	docker-compose -f docker-compose.dev.yml up --build

docker-down:
	docker-compose -f docker-compose.dev.yml down

# Formatting and linting
format:
	dotnet format payment.slnx

# Help
help:
	@echo "Available targets:"
	@echo "  build          - Build the solution"
	@echo "  build-release  - Build in Release mode"
	@echo "  test           - Run tests"
	@echo "  test-coverage  - Run tests with coverage"
	@echo "  clean          - Clean build artifacts"
	@echo "  restore        - Restore NuGet packages"
	@echo "  run            - Run the application"
	@echo "  run-dev        - Run in development mode"
	@echo "  watch          - Run with file watching"
	@echo "  publish        - Publish for deployment"
	@echo "  migration-add  - Add new EF Core migration"
	@echo "  migration-update - Update database"
	@echo "  migration-remove - Remove last migration"
	@echo "  docker-build   - Build Docker image"
	@echo "  docker-run     - Run Docker container"
	@echo "  docker-up      - Start with docker-compose"
	@echo "  docker-down    - Stop docker-compose"
	@echo "  format         - Format code"
