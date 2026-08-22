# Docker Setup for Payment Service

This document explains how to run the Payment Service using Docker and Docker Compose with Traefik reverse proxy.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v3.8 or higher
- At least 4GB of available RAM
- Traefik reverse proxy running on `dokploy-network` (for production)

## Quick Start

### 1. Environment Configuration

Copy the example environment file and configure your settings:

```bash
cp .env.example .env
```

Edit `.env` file with your actual values:
- **HOST**: Your domain name (e.g., `parvazorg.minisource.ir`)
- **HOST_PREFIX**: API path prefix (e.g., `/api/payment`)
- **PORT**: Application port (default: 4005)
- **ZarinPal Merchant ID**
- **Snapp API Key and Terminal ID**
- Other gateway credentials

### 2. Build and Run

**Production Mode (with Traefik):**
```bash
docker-compose up -d
```

**Development Mode (without Traefik):**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### 3. Initialize Database

After the containers are running, apply EF Core migrations:

```bash
# Connect to the payment-api container
docker exec -it payment-api bash

# Run migrations
dotnet ef database update
```

Or if you prefer to run from host:
```bash
# Set connection string to Docker SQL Server
dotnet ef database update --project ./Infrastructure/Infrastructure.csproj --startup-project ./payment/Presentaion.csproj
```

## Services

### Payment API
- **Local URL:** http://localhost:4005
- **Production URL:** https://parvazorg.minisource.ir/api/payment (via Traefik)
- **Swagger:** http://localhost:4005/swagger (or via Traefik path)
- **Health Check:** http://localhost:4005/health

### SQL Server
- **Host:** localhost
- **Port:** 1433
- **User:** sa
- **Password:** YourPassword123! (change in .env)
- **Database:** PaymentDb

## Traefik Configuration

The payment service is configured to work with Traefik reverse proxy with the following features:

### Features
- ✅ **HTTPS/TLS**: Automatic HTTPS via Let's Encrypt
- ✅ **Path Prefix Stripping**: Removes `/api/payment` prefix before forwarding to service
- ✅ **Host-based Routing**: Routes based on domain name
- ✅ **Load Balancing**: Configured for the internal service port

### Traefik Labels Explained

```yaml
traefik.enable=true
# Enable Traefik for this service

traefik.http.routers.payment.rule=Host(`parvazorg.minisource.ir`) && PathPrefix(`/api/payment`)
# Route requests to parvazorg.minisource.ir/api/payment/* to this service

traefik.http.routers.payment.entrypoints=websecure
# Use HTTPS entrypoint (port 443)

traefik.http.routers.payment.tls=true
# Enable TLS/SSL

traefik.http.routers.payment.tls.certresolver=letsencrypt
# Use Let's Encrypt for SSL certificates

traefik.http.routers.payment.middlewares=payment-strip
# Apply middleware to strip path prefix

traefik.http.middlewares.payment-strip.stripprefix.prefixes=/api/payment
# Remove /api/payment from URL before forwarding

traefik.http.services.payment.loadbalancer.server.port=4005
# Internal service port
```

### Environment Variables for Traefik

| Variable | Description | Default |
|----------|-------------|---------|
| `HOST` | Domain name for routing | `parvazorg.minisource.ir` |
| `HOST_PREFIX` | URL path prefix | `/api/payment` |
| `PORT` | Internal service port | `4005` |

### Example Routing

With default configuration:
- **External Request**: `https://parvazorg.minisource.ir/api/payment/v1/payments`
- **Traefik Processing**: Strips `/api/payment`
- **Forwarded to Service**: `http://payment-api:4005/v1/payments`

## Docker Compose Files

### docker-compose.yml (Production)
- Uses production-optimized settings
- Includes Traefik labels for reverse proxy
- Connected to external `dokploy-network`
- HTTPS with Let's Encrypt
- Suitable for deployment

### docker-compose.dev.yml (Development)
- Development environment
- No Traefik labels (direct access)
- Hot reload support (when using volume mounts)
- Isolated network for development
- Easier debugging
