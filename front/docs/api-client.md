# API Client

> Last updated: 2026-07-08

## Overview

The API client is built on Axios with request/response interceptors for
authentication, tenant context, and standardized error handling.

## Architecture

```
/api/client.ts
  ├── createClient()
  │   ├── axios.create({ baseURL, timeout, headers })
  │   ├── Request interceptor: attach auth + tenant headers
  │   └── Response interceptor: parse errors into ApiError
  ├── get<T>(url, params?)       → GET request
  ├── post<T>(url, body?)        → POST request
  ├── patch<T>(url, body?)       → PATCH request
  └── del<T>(url)                → DELETE request

/api/*.ts (typed API modules)
  ├── dashboard.ts
  ├── wallets.ts
  ├── outbox.ts
  ├── webhooks.ts
  ├── audit.ts
  ├── system-health.ts
  └── ... (one per feature module)
```

## Base URL

Configured via environment variable:

```
NEXT_PUBLIC_PAYMENT_API_URL=http://localhost:4008
```

Default fallback: `http://localhost:4008`

## Headers

| Header | Source | When |
|--------|--------|------|
| `Authorization: Bearer <token>` | `localStorage.getItem('accessToken')` | Always (if token exists) |
| `X-Tenant-Id: <id>` | `localStorage.getItem('X-Tenant-Id')` | Always (if set) |
| `X-Application-Code: <code>` | `localStorage.getItem('X-Application-Code')` | Always (default: `payment-admin`) |
| `Content-Type: application/json` | Default | Always |

Headers are attached automatically by the request interceptor.

## API Module Pattern

Every feature module follows this pattern:

```typescript
import { get, post, patch, del } from '@/api/client';

// Types
export interface SomeItemDto { ... }
export interface SomeDetailDto extends SomeItemDto { ... }
export interface CreateRequest { ... }
export interface UpdateRequest { ... }

// API calls
export async function listItems(params?: Record<string, unknown>) {
  return get<{ items: SomeItemDto[]; total: number }>('/api/v1/admin/items', params);
}

export async function getItem(id: string) {
  return get<SomeDetailDto>(`/api/v1/admin/items/${id}`);
}

export async function createItem(req: CreateRequest) {
  return post<{ id: string }>('/api/v1/admin/items', req);
}
```

## Pagination

List endpoints accept:
- `skip` — offset (0-based)
- `take` — page size (default 20)
- `cursor` — optional cursor for cursor-based pagination

All list responses include:
```typescript
{ items: T[]; total: number }
```

The `total` field is used by the `Pagination` component.

## Error Handling

See `docs/error-handling.md` for full details.

Errors are typed as `ApiError` with:
- `status` — HTTP status code
- `code` — Backend error code
- `message` — Technical message
- `userMessage` — User-facing message
- `fieldErrors` — Validation errors per field
- `requestId` / `correlationId` — Trace identifiers

## API Module Inventory

| Module | File | Key Functions |
|--------|------|---------------|
| Dashboard | `api/dashboard.ts` | Get overview, charts, latest items |
| Reports | `api/reports.ts` | Get report data |
| Wallets | `api/wallets.ts` | List/get/adjust wallets |
| Ledger | `api/ledger.ts` | List/get ledger entries |
| Payment Intents | `api/payment-intents.ts` | List/get/verify intents |
| Payment Transactions | `api/payment-transactions.ts` | List/get transactions |
| Gateway Providers | `api/gateway-providers.ts` | List/get providers |
| Gateway Configs | `api/gateway-configs.ts` | CRUD for configs |
| Gateway Routing | `api/gateway-routing.ts` | CRUD for routing policies/rules |
| Payment Links | `api/payment-links.ts` | List/get/update links |
| Payout Accounts | `api/payout-accounts.ts` | List/get/verify/reject/disable |
| Withdrawals | `api/withdrawals.ts` | List/get/approve/reject/mark-paid/failed |
| Security | `api/security.ts` | Get security overview |
| Ledger Integrity | `api/ledger-integrity.ts` | Run checks, get issues |
| Wallet Consistency | `api/wallet-consistency.ts` | Run checks, get issues |
| Reconciliation | `api/reconciliation.ts` | List/run/resolve batches/items |
| Risk | `api/risk.ts` | List/get/manage risk cases |
| Limit Usage | `api/limit-usage.ts` | List/get limit usage |
| Admin Approvals | `api/admin-approvals.ts` | CRUD + approve/reject/execute |
| Approval Policies | `api/approval-policies.ts` | CRUD for policies |
| Events Overview | `api/events.ts` | Get events overview |
| Outbox | `api/outbox.ts` | List/get/retry/skip/dead-letter |
| Webhooks | `api/webhooks.ts` | CRUD + enable/disable/regenerate-secret |
| Webhook Deliveries | `api/webhook-deliveries.ts` | List/get/retry/dead-letter |
| Event Routing | `api/event-routing.ts` | CRUD + enable/disable |
| Audit | `api/audit.ts` | List/get/verify hash chain |
| System Health | `api/system-health.ts` | Get health/readiness/dependencies/dispatchers |
| Settings | `api/settings.ts` | Get/update settings |

---

> This document is part of Phase 7.8 final polish documentation.
