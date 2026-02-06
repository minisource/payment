# Minisource Payment Service

Multi-tenant payment processing microservice built with .NET 10. Handles payment gateways, transactions, and billing operations.

## Features

- 💳 **Multi-Gateway Support** - ZarinPal, Stripe, PayPal integration ready
- 🏢 **Multi-Tenant** - Complete tenant isolation for payments
- 📊 **Transaction Management** - Track and manage all transactions
- 🔄 **Webhook Handling** - Process payment provider callbacks
- 📝 **Audit Trail** - Complete payment history logging
- 🔐 **OAuth2 Authentication** - Secure service-to-service auth

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                Payment Service (:4005)                   │
├─────────────────────────────────────────────────────────┤
│  ┌────────────────┐  ┌────────────────┐                 │
│  │   Application  │  │    Domain      │                 │
│  │    Services    │  │    Entities    │                 │
│  └───────┬────────┘  └───────┬────────┘                 │
│          │                   │                          │
│  ┌───────▼───────────────────▼────────┐                 │
│  │         Infrastructure             │                 │
│  │  ┌─────────┐  ┌─────────────────┐  │                 │
│  │  │ Payment │  │    Database     │  │                 │
│  │  │ Gateway │  │   Repository    │  │                 │
│  │  └─────────┘  └─────────────────┘  │                 │
│  └────────────────────────────────────┘                 │
└─────────────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 15+
- Docker & Docker Compose (optional)

### Development

```bash
# Clone repository
git clone https://github.com/minisource/payment.git
cd payment

# Copy environment file
cp .env.example .env

# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update --project Infrastructure

# Run the service
dotnet run --project payment
```

### Docker

```bash
# Build and run with Docker Compose
docker-compose -f docker-compose.dev.yml up --build

# Or use the scripts
./docker.sh up     # Linux/Mac
docker.bat up      # Windows
```

## Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `PORT` | Service port | `4005` |
| `DATABASE_URL` | PostgreSQL connection | Required |
| `AUTH_SERVICE_URL` | Auth service URL | `http://auth:9001` |
| `CLIENT_ID` | OAuth client ID | Required |
| `CLIENT_SECRET` | OAuth client secret | Required |
| `ZARINPAL_MERCHANT_ID` | ZarinPal merchant | Optional |
| `STRIPE_SECRET_KEY` | Stripe API key | Optional |

## API Endpoints

### Payment Operations

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/payments` | Create payment |
| GET | `/api/v1/payments/{id}` | Get payment |
| GET | `/api/v1/payments` | List payments |
| POST | `/api/v1/payments/{id}/verify` | Verify payment |
| POST | `/api/v1/payments/{id}/refund` | Refund payment |

### Webhooks

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/webhooks/zarinpal` | ZarinPal callback |
| POST | `/api/v1/webhooks/stripe` | Stripe webhook |

### Admin Operations

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/admin/transactions` | List all transactions |
| GET | `/api/v1/admin/stats` | Payment statistics |

## Project Structure

```
payment/
├── Application/
│   ├── DTOs/              # Data transfer objects
│   ├── Options/           # Configuration options
│   └── Services/          # Business logic
├── Domain/
│   ├── Entities/          # Domain entities
│   ├── Enums/             # Enumerations
│   └── Interfaces/        # Contracts
├── Infrastructure/
│   ├── Data/              # EF Core context
│   ├── Repositories/      # Data access
│   └── Services/          # External integrations
├── payment/
│   ├── Controllers/       # API controllers
│   └── Program.cs         # Entry point
├── docker-compose.yml
├── docker-compose.dev.yml
└── Dockerfile
```

## Payment Flow

```
1. Client creates payment request
         │
         ▼
2. Payment service generates gateway URL
         │
         ▼
3. User redirected to payment gateway
         │
         ▼
4. Gateway processes payment
         │
         ▼
5. Gateway calls webhook with result
         │
         ▼
6. Payment service verifies and updates status
         │
         ▼
7. Client receives confirmation
```

## Adding Payment Gateways

1. Create gateway service in `Infrastructure/Services/`
2. Implement `IPaymentGateway` interface
3. Register in dependency injection
4. Add configuration options

## Build Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Publish
dotnet publish -c Release -o ./publish

# Docker build
docker build -t minisource/payment .
```

## Environment Files

- `.env.example` - Template configuration
- `.env` - Local development (git ignored)

## Dependencies

- **ASP.NET Core 10** - Web framework
- **Entity Framework Core** - ORM
- **Minisource.Common** - Shared utilities
- **Minisource.Sdk** - Service clients

## License

MIT