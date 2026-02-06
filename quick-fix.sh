#!/bin/bash

# Quick Fix and Deploy - Payment Service
# This script applies all fixes and redeploys the service

echo "?? Applying fixes and redeploying Payment Service..."
echo ""

# Stop existing containers
echo "1/6 Stopping containers..."
docker-compose down

# Remove old volume to fix permission issues
echo "2/6 Removing old DataProtection volume..."
docker volume rm payment-dataprotection 2>/dev/null || true

# Build with no cache
echo "3/6 Building image..."
docker-compose build --no-cache

# Start services
echo "4/6 Starting services..."
docker-compose up -d

# Wait for services
echo "5/6 Waiting for health checks (40 seconds)..."
sleep 40

# Check status
echo "6/6 Checking status..."
docker-compose ps
echo ""

# Test endpoints
echo "Testing endpoints..."
echo ""

# Local health
if curl -s -f http://localhost:4005/health > /dev/null 2>&1; then
    echo "? Local health: http://localhost:4005/health"
else
    echo "? Local health check failed"
fi

# Local swagger
if curl -s -f http://localhost:4005/swagger/v1/swagger.json > /dev/null 2>&1; then
    echo "? Local Swagger: http://localhost:4005/swagger/index.html"
else
    echo "? Local Swagger failed"
fi

echo ""
echo "?? Deployment complete!"
echo ""
echo "Check logs: docker-compose logs -f payment"
echo "Check Swagger: http://localhost:4005/swagger/index.html"
echo "Check via Traefik: https://parvazorg.minisource.ir/api/payment/swagger/index.html"
echo ""
