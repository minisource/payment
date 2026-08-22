# Environment Configuration

Copy `.env.example` to `.env.local` and configure your local environment.

## Required Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `NEXT_PUBLIC_PAYMENT_API_URL` | Payment backend base URL | `http://localhost:4008` | Yes |
| `NEXT_PUBLIC_AUTH_API_URL` | Auth service base URL | `http://localhost:8080` | No* |
| `NEXT_PUBLIC_APP_NAME` | Application display name | `MiniSource Payment Admin` | No |
| `NEXT_PUBLIC_APP_DESCRIPTION` | Meta description | `Payment Service Operations Console` | No |
| `NEXT_PUBLIC_DEV_MOCK_AUTH` | Enable dev mock auth (super_admin) | — | Dev only |

*Auth API URL is only needed if the auth service provides user profile or permission endpoints.

## Dev Mock Auth

When `NEXT_PUBLIC_DEV_MOCK_AUTH=true`, the app will simulate an authenticated
super_admin user without needing a real auth backend. This is useful for
frontend development when the auth service is not running.

**Never set this in production.**

```bash
# .env.local (development)
NEXT_PUBLIC_API_URL=http://localhost:4008
NEXT_PUBLIC_DEV_MOCK_AUTH=true
```

## Headers

The API client automatically attaches these headers to every request:

| Header | Source | Purpose |
|--------|--------|---------|
| `Authorization: Bearer <token>` | `localStorage.accessToken` | Auth token |
| `X-Tenant-Id: <id>` | `localStorage.X-Tenant-Id` or tenant context | Tenant context |
| `X-Application-Code: payment-admin` | Hardcoded default / `localStorage.X-Application-Code` | Application identity |
| `Content-Type: application/json` | Default | JSON API |

## Environment Files

| File | Purpose |
|------|---------|
| `.env.example` | Reference template (checked into git) |
| `.env.local` | Local development overrides (git-ignored) |
| `.env.production` | Production environment (git-ignored) |
| `.env.development` | Development defaults (git-ignored) |

---

> Last updated: 2026-07-07
