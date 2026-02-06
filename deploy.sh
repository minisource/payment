#!/bin/bash

# Payment Service - Fix and Deploy Script

set -e

echo "?? Payment Service - Fix and Deploy"
echo "===================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_step() {
    echo -e "${BLUE}? $1${NC}"
}

print_success() {
    echo -e "${GREEN}? $1${NC}"
}

print_error() {
    echo -e "${RED}? $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}? $1${NC}"
}

# Step 1: Check Docker
print_step "Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi
print_success "Docker is running"

# Step 2: Check .env file
print_step "Checking environment configuration..."
if [ ! -f .env ]; then
    print_warning ".env file not found. Creating from template..."
    cp .env.example .env
    print_warning "Please edit .env file with your actual values before deploying!"
    echo ""
    echo "Required values:"
    echo "  - HOST"
    echo "  - ZARINPAL_MERCHANT_ID"
    echo "  - SNAPP_API_KEY and SNAPP_TERMINAL_ID"
    echo "  - SA_PASSWORD (change from default)"
    echo ""
    read -p "Press Enter when ready to continue..."
else
    print_success ".env file exists"
fi

# Step 3: Stop existing containers
print_step "Stopping existing containers..."
docker-compose down || true
print_success "Containers stopped"

# Step 4: Clean old volume (optional)
read -p "Do you want to recreate DataProtection volume (fixes permission issues)? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_step "Removing old DataProtection volume..."
    docker volume rm payment-dataprotection 2>/dev/null || true
    print_success "Old volume removed"
fi

# Step 5: Build image
print_step "Building Docker image (this may take a few minutes)..."
docker-compose build --no-cache
print_success "Image built successfully"

# Step 6: Start services
print_step "Starting services..."
docker-compose up -d
print_success "Services started"

# Step 7: Wait for health checks
print_step "Waiting for services to be healthy (40 seconds)..."
sleep 10

for i in {1..30}; do
    if docker-compose ps | grep -q "healthy"; then
        print_success "Services are healthy"
        break
    fi
    echo -n "."
    sleep 1
done
echo ""

# Step 8: Show status
print_step "Container status:"
docker-compose ps

# Step 9: Check logs
print_step "Checking for errors in logs..."
if docker-compose logs payment --tail 20 | grep -iq "error\|exception\|permission denied\|unauthorized"; then
    print_warning "Found potential errors in logs:"
    docker-compose logs payment --tail 20 | grep -i "error\|exception\|permission denied\|unauthorized"
else
    print_success "No errors found in logs"
fi

# Step 10: Test endpoints
echo ""
print_step "Testing endpoints..."

# Test local health
if curl -s -f http://localhost:4005/health > /dev/null 2>&1; then
    print_success "Local health check: http://localhost:4005/health ?"
else
    print_error "Local health check failed"
fi

# Test local swagger
if curl -s -f http://localhost:4005/swagger/v1/swagger.json > /dev/null 2>&1; then
    print_success "Local Swagger: http://localhost:4005/swagger/index.html ?"
else
    print_warning "Local Swagger not accessible (might still be starting up)"
fi

# Get HOST from .env
if [ -f .env ]; then
    HOST=$(grep "^HOST=" .env | cut -d '=' -f2 | tr -d '"' | tr -d "'")
    HOST_PREFIX=$(grep "^HOST_PREFIX=" .env | cut -d '=' -f2 | tr -d '"' | tr -d "'")
    
    if [ ! -z "$HOST" ] && [ "$HOST" != "parvazorg.minisource.ir" ]; then
        echo ""
        print_step "Testing Traefik access..."
        
        # Test via Traefik
        if curl -s -k -f https://${HOST}${HOST_PREFIX}/health > /dev/null 2>&1; then
            print_success "Traefik health check: https://${HOST}${HOST_PREFIX}/health ?"
        else
            print_warning "Traefik health check failed (Traefik might not be configured yet)"
        fi
        
        if curl -s -k -f https://${HOST}${HOST_PREFIX}/swagger/v1/swagger.json > /dev/null 2>&1; then
            print_success "Traefik Swagger: https://${HOST}${HOST_PREFIX}/swagger/index.html ?"
        else
            print_warning "Traefik Swagger not accessible (check Traefik configuration)"
        fi
    fi
fi

# Step 11: Database migrations (optional)
echo ""
read -p "Do you want to run database migrations now? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_step "Running database migrations..."
    if docker exec -it payment-api dotnet ef database update; then
        print_success "Migrations completed"
    else
        print_error "Migration failed. Check logs for details."
    fi
fi

# Summary
echo ""
echo "===================================="
echo -e "${GREEN}? Deployment Complete!${NC}"
echo "===================================="
echo ""
echo "?? Access Points:"
echo "  • Health:  http://localhost:4005/health"
echo "  • Swagger: http://localhost:4005/swagger/index.html"
if [ ! -z "$HOST" ]; then
    echo "  • Traefik Health:  https://${HOST}${HOST_PREFIX}/health"
    echo "  • Traefik Swagger: https://${HOST}${HOST_PREFIX}/swagger/index.html"
fi
echo ""
echo "?? Monitoring Commands:"
echo "  • View logs:   docker-compose logs -f payment"
echo "  • Check status: docker-compose ps"
echo "  • Restart:     docker-compose restart"
echo ""
echo "?? For troubleshooting, see: TROUBLESHOOTING.md"
echo ""
