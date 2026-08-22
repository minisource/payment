# Payment Admin Frontend

Minisource Payment Service — Admin Dashboard & Operations Console.

**Phase 7.8 Complete** — All 8 phases of the admin frontend are now implemented.

## Stack

- **Framework:** Next.js 15 (App Router)
- **Language:** TypeScript 5.7 (strict mode)
- **Styling:** Tailwind CSS 3.4 + shadcn/ui primitives
- **State Management:** @tanstack/react-query (server state) + Zustand 5 (client state)
- **HTTP:** Axios with interceptors (auth/tenant/app headers, error parsing)
- **Icons:** Lucide React
- **Charts:** Recharts
- **Forms:** React Hook Form + Zod
- **Tests:** Vitest + @testing-library/react (65 tests)

## Getting Started

```bash
npm install
cp .env.example .env.local
# For dev without auth backend, add: NEXT_PUBLIC_DEV_MOCK_AUTH=true
npm run dev
```

## Available Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start Next.js dev server |
| `npm run dev:turbo` | Start with Turbopack |
| `npm run build` | Production build |
| `npm run type-check` | TypeScript type checking |
| `npm run lint` | ESLint |
| `npm run format` | Prettier format |
| `npm run test` | Run Vitest tests (65 tests) |
| `npm run test:coverage` | Run tests with coverage |

## Project Structure

```
src/
  __tests__/          # 65 Vitest tests
  api/                # 30+ typed API modules
    client.ts         # Axios instance with interceptors
  app/                # Next.js App Router
    (main)/           # Authenticated routes
      admin/          # 35+ feature pages (all 7 phases)
      dashboard/      # Full financial dashboard
    layout.tsx        # Root layout
  components/
    providers/        # Query/Theme/Auth/Permission/Tenant providers
    shared/           # 20+ reusable business components
    ui/               # shadcn-style primitives (Button, Card, etc.)
  config/             # App constants
  hooks/              # usePermission, useTenant
  lib/                # Utilities (cn, formatCurrency, etc.)
  stores/             # Zustand auth store (persisted)
  styles/             # Global CSS
docs/                 # 20+ documentation files
```

## Phases Completed

| Phase | Focus | Status |
|-------|-------|--------|
| **7.1** | Foundation, Auth, Layout, API Client, Permissions | ✅ |
| **7.2** | Dashboard, Reports Shell & Shared Admin Components | ✅ |
| **7.3** | Wallets, Ledger, Payment Intents & Transactions | ✅ |
| **7.4** | Gateway Providers, Configs & Routing | ✅ |
| **7.5** | Payment Links, Payout Accounts, Withdrawals | ✅ |
| **7.6** | Financial Security, Risk, Reconciliation, Approvals | ✅ |
| **7.7** | Outbox, Webhooks, Event Routing, Audit, System Health | ✅ |
| **7.8** | Polish, Permissions Hardening, Docs, Production Readiness | ✅ |

## Key Safety Principles

- **No secrets exposed** — Gateway/webhook secrets never displayed; one-time display only on create
- **Payload redaction** — `SafePayloadViewer` auto-redacts secret/token/card/IBAN fields
- **Read-only audit** — Audit logs and ledger entries have no edit/delete UI
- **Permission guards** — Every route and sensitive action is permission-protected
- **Confirmation required** — All financial/operational actions require confirmation
- **No fake success** — UI updates only after backend API confirms

## Environment Variables

| Variable | Description |
|----------|-------------|
| `NEXT_PUBLIC_PAYMENT_API_URL` | Payment backend base URL |
| `NEXT_PUBLIC_AUTH_API_URL` | Auth service base URL |
| `NEXT_PUBLIC_APP_NAME` | Application display name |
| `NEXT_PUBLIC_DEV_MOCK_AUTH` | Enable dev mock auth (dev only — do NOT set in production) |

## Documentation

| Document | Description |
|----------|-------------|
| [admin-front-phases.md](docs/admin-front-phases.md) | Full phased plan and deliverables |
| [routes.md](docs/routes.md) | Complete route inventory with permissions |
| [permissions.md](docs/permissions.md) | Permission system guide and full mapping |
| [api-client.md](docs/api-client.md) | API client architecture and module inventory |
| [api-gaps.md](docs/api-gaps.md) | Backend API gaps organized by module |
| [security-review.md](docs/security-review.md) | Security audit results |
| [secret-handling.md](docs/secret-handling.md) | Webhook/gateway secret handling patterns |
| [payload-redaction.md](docs/payload-redaction.md) | SafePayloadViewer redaction behavior |
| [operation-safety.md](docs/operation-safety.md) | Confirmation patterns for all actions |
| [error-handling.md](docs/error-handling.md) | Error handling patterns and components |
| [testing.md](docs/testing.md) | Test setup, patterns, and coverage |
| [production-readiness.md](docs/production-readiness.md) | Production readiness checklist |
| [environment.md](docs/environment.md) | Environment configuration guide |
| [development.md](docs/development.md) | Development guide and conventions |

---

> Last updated: 2026-07-08
