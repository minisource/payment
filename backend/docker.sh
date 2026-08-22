#!/bin/bash

# Payment Service Docker Management Script

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
print_success() {
    echo -e "${GREEN}? $1${NC}"
}

print_error() {
    echo -e "${RED}? $1${NC}"
}

print_info() {
    echo -e "${YELLOW}? $1${NC}"
}

# Check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    print_success "Docker is running"
}

# Build the services
build_services() {
    print_info "Building services..."
    docker-compose build
    print_success "Services built successfully"
}

# Start services
start_services() {
    local compose_file=${1:-docker-compose.yml}
    print_info "Starting services using $compose_file..."
    
    if [ "$compose_file" == "docker-compose.yml" ]; then
        docker-compose up -d
    else
        docker-compose -f $compose_file up -d
    fi
    
    print_success "Services started successfully"
    print_info "Waiting for services to be healthy..."
    sleep 10
    docker-compose ps
}

# Stop services
stop_services() {
    print_info "Stopping services..."
    docker-compose down
    print_success "Services stopped successfully"
}

# View logs
view_logs() {
    local service=${1:-payment}
    print_info "Showing logs for $service..."
    docker-compose logs -f $service
}

# Run migrations
run_migrations() {
    print_info "Running database migrations..."
    docker exec -it payment-api dotnet ef database update || {
        print_error "Failed to run migrations. Trying alternative method..."
        dotnet ef database update --project ./Infrastructure/Infrastructure.csproj --startup-project ./payment/Presentaion.csproj
    }
    print_success "Migrations completed successfully"
}

# Health check
health_check() {
    print_info "Checking service health..."
    
    # Check payment API
    if curl -f http://localhost:4005/health > /dev/null 2>&1; then
        print_success "Payment API is healthy"
    else
        print_error "Payment API is not responding"
    fi
    
    # Check SQL Server
    if docker exec payment-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword123! -C -Q "SELECT 1" > /dev/null 2>&1; then
        print_success "SQL Server is healthy"
    else
        print_error "SQL Server is not responding"
    fi
}

# Clean up
cleanup() {
    print_info "Cleaning up Docker resources..."
    read -p "This will remove all containers, volumes, and data. Continue? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose down -v
        docker system prune -f
        print_success "Cleanup completed"
    else
        print_info "Cleanup cancelled"
    fi
}

# Show help
show_help() {
    cat << EOF
Payment Service Docker Management Script

Usage: ./docker.sh [COMMAND] [OPTIONS]

Commands:
    start [dev]      Start services (use 'dev' for development mode)
    stop             Stop services
    restart [dev]    Restart services
    build            Build Docker images
    logs [service]   View logs (default: payment service)
    migrate          Run database migrations
    health           Check service health
    clean            Clean up all Docker resources
    shell [service]  Open shell in container (default: payment-api)
    help             Show this help message

Examples:
    ./docker.sh start           # Start in production mode
    ./docker.sh start dev       # Start in development mode
    ./docker.sh logs payment    # View payment service logs
    ./docker.sh logs sqlserver  # View SQL Server logs
    ./docker.sh migrate         # Run database migrations
    ./docker.sh shell           # Open shell in payment-api container

EOF
}

# Main script
main() {
    check_docker

    case "$1" in
        start)
            if [ "$2" == "dev" ]; then
                start_services "docker-compose.dev.yml"
            else
                start_services "docker-compose.yml"
            fi
            ;;
        stop)
            stop_services
            ;;
        restart)
            stop_services
            if [ "$2" == "dev" ]; then
                start_services "docker-compose.dev.yml"
            else
                start_services "docker-compose.yml"
            fi
            ;;
        build)
            build_services
            ;;
        logs)
            view_logs $2
            ;;
        migrate)
            run_migrations
            ;;
        health)
            health_check
            ;;
        clean)
            cleanup
            ;;
        shell)
            service=${2:-payment-api}
            print_info "Opening shell in $service..."
            docker exec -it $service bash
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            show_help
            exit 1
            ;;
    esac
}

# Run main script
main "$@"
