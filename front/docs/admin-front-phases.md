# Payment Admin Front — Phased Implementation Plan

This document outlines the phased approach for building the Payment Admin Frontend.

## Phase Overview

| Phase | Focus | Status |
|-------|-------|--------|
| **7.1** | Foundation, Auth, Layout, API Client, Permissions | ✅ Complete |
| **7.2** | Dashboard, Reports Shell & Shared Admin Components | ✅ Complete |
| **7.3** | Wallets, Ledger, Payment Intents & Gateway Transactions | ✅ Complete |
| **7.4** | Gateway Providers, Gateway Configs & Routing Management | ✅ Complete |
| **7.5** | Payment Links, Payout Accounts & Withdrawal Queue | ✅ Complete |
| **7.6** | Financial Security, Risk, Reconciliation & Admin Approvals | ✅ Complete |
| **7.7** | Outbox, Webhooks, Event Routing, Audit Logs & System Health | ✅ Complete |
| **7.8** | Polish, Permissions Hardening, Tests, Docs & Production Readiness | 📅 Planned |

---

## Phase 7.1 — Foundation

**Objective:** Clean foundation with auth abstraction, permission system, tenant context,
API client, error handling, and placeholder pages.

### Delivered in Phase 7.1

- **Auth Store** (`stores/auth.store.ts`) — Zustand-based auth state with persistence, following `auth/front` pattern
- **Permission System** — `PermissionProvider`, `usePermission`, `PermissionGuard`, `RoutePermissionGuard`, `ActionPermissionGuard`
- **Tenant Context** — `TenantProvider`/`useTenant`, `ApplicationProvider`/`useApplication`
- **API Client** (`api/client.ts`) — Axios-based with auth token injection, tenant/application headers, standard error parsing
- **Error Handling** — `ApiError` type, error parser, 403 page, 404 page, global error boundary
- **App Layout** — Sidebar (permission-filtered), topbar, tenant selector placeholder, user info, logout
- **Placeholder Pages** — 16 feature pages with phase indicators and required permissions
- **Tests** — 12 tests across app rendering, shared components, auth store, permission guards, API client
- **Docs** — Full documentation set

---

## Phase 7.2 — Dashboard & Components

**Objective:** Build the main dashboard with real data and create shared admin components
(DataTable, FilterBar, DateRangeFilter, MetricCard, ChartCard, etc.).

### Delivered in Phase 7.2

- **Dashboard Page** — Full dashboard with permission-aware metric sections (financial, payments, withdrawals, gateways, security, events), 5 recharts (bar, line, pie), 5 latest-activity tables, date range filter, refresh button, and quick navigation cards
- **Reports Shell** — `/admin/reports` landing page with 7 category cards linking to individual report shell pages with filter+export UI
- **MetricCard** — Reusable component with loading/error/value states, trend indicator, icon support
- **ChartCard** — Wrapper for recharts with loading/empty/error states and retry
- **Enhanced StatusBadge** — Comprehensive mappings for 40+ statuses across payment intent, gateway, wallet, withdrawal, risk, reconciliation, outbox, webhook, approval domains
- **Enhanced MoneyAmount** — Negative values, skeleton loading, copyable, currency symbols
- **DataTable** — Reusable table with loading/empty/error/success states, compact mode, sortable headers, pagination
- **FilterBar Components** — SearchInput, DateRangeFilter, RefreshButton, ExportButton, FilterBar wrapper
- **ReportShell** — Reusable layout for individual report pages
- **API Modules** — `api/dashboard.ts` (11 endpoints) + `api/reports.ts` (8 endpoints)
- **Tests** — 12 tests covering MetricCard, MoneyAmount, StatusBadge, DataTable, DateRangeFilter, dashboard render, reports landing

---

## Phase 7.3 — Wallets, Ledger & Payments

**Objective:** First real financial feature pages.

### Delivered in Phase 7.3

- Wallet list with filters (ownerType, currency, status, balance range)
- Wallet detail with available/locked/pending balances
- Ledger explorer (search, filter by type/currency/date, hash status)
- Payment intents list/detail with status timeline
- Gateway transaction detail with safe payload viewer
- Manual verify UI
- Admin wallet adjustments (credit/debit with confirmation + reason)
- API modules: wallets, ledger, payment-intents
- Tests: wallet list, wallet detail, ledger explorer, payment intents, wallet adjustments

---

## Phase 7.4 — Gateways

**Objective:** Gateway management CRUD.

### Delivered in Phase 7.4

- Provider catalog with status and supported currencies
- Gateway config CRUD (create/edit/delete)
- Secret masking (never display existing values, allow update)
- Enable/disable/test gateway
- Routing policies (priority/random/weighted_random UI)
- Routing rules management
- Production gateway change warnings
- API modules: gateway-providers, gateway-configs, gateway-routing

---

## Phase 7.5 — Withdrawals & Payouts

**Objective:** Withdrawal admin queue and payout account management.

### Delivered in Phase 7.5

- Withdrawal queue with filter tabs
- Approve/reject/mark-paid/mark-failed actions
- Reason dialogs for sensitive actions
- Bank tracking number input for mark-paid
- Release funds option for mark-failed
- Payout account list with masked card/IBAN
- Verify/reject/disable payout accounts
- Payment links list/detail with pause/resume/disable

---

## Phase 7.6 — Security Console

**Objective:** Financial security monitoring and approvals.

### Delivered in Phase 7.6

- Risk cases list/detail with severity badges
- Reconciliation batches and items with resolve/false-positive
- Ledger integrity checks (run wallet check, run tenant deep check)
- Wallet consistency issues
- Limit usage views
- Admin approval requests with maker-checker workflow
- Approval policies CRUD
- Requester cannot approve own request rule

---

## Phase 7.7 — Events & Audit

**Objective:** Event delivery and operational infrastructure console.

### Delivered in Phase 7.7

#### Routes Implemented
| Route | Description |
|-------|-------------|
| `/admin/events` | Events overview dashboard with summary cards and quick links |
| `/admin/events/outbox` | Outbox events list with filters and actions |
| `/admin/events/outbox/:eventId` | Outbox event detail with redacted payload |
| `/admin/outbox` | Route alias → redirects to `/admin/events/outbox` |
| `/admin/events/webhooks` | Webhook subscriptions list |
| `/admin/events/webhooks/new` | Create webhook with one-time secret display |
| `/admin/events/webhooks/:id` | Webhook detail with enable/disable/regenerate/deactivate actions |
| `/admin/events/webhooks/:id/edit` | Edit webhook subscription |
| `/admin/webhooks` | Route alias → redirects to `/admin/events/webhooks` |
| `/admin/events/webhook-deliveries` | Webhook deliveries list with retry/dead-letter |
| `/admin/events/webhook-deliveries/:id` | Webhook delivery detail with redacted request/response |
| `/admin/events/routing` | Event routing rules list |
| `/admin/events/routing/new` | Create routing rule |
| `/admin/events/routing/:id` | Routing rule detail with target config viewer |
| `/admin/events/routing/:id/edit` | Edit routing rule |
| `/admin/audit` | Audit logs explorer with hash status |
| `/admin/audit/:auditLogId` | Audit log detail with snapshots and hash chain info |
| `/admin/system-health` | System health dashboard with dependencies and dispatchers |

#### Outbox
- Full list with filters (status, search, pagination) and data table columns
- Event detail with summary, timeline, request info, redacted payload via SafePayloadViewer
- Delivery attempts table (if API supports)
- Retry/skip/dead-letter actions with confirmation dialogs
- Skip/dead-letter require reason input
- Permission guards on all actions

#### Webhooks
- Subscription list with filters and status badges
- Create form with URL validation, event type selection, one-time secret modal
- Edit form with pre-filled data, no existing secret displayed
- Detail page with enable/disable/delete/regenerate-secret actions
- One-time secret display on create and regenerate (never persisted in UI)
- Secret status shown as configured/not configured only
- Permission guards on all actions

#### Webhook Deliveries
- Delivery list with filters and HTTP status display
- Delivery detail with summary, timeline, attempt history, redacted request/response
- Retry/dead-letter actions with confirmation
- Request headers redacted; signature/secret headers hidden
- Response body truncated and safe

#### Event Routing
- Rules list with target type badges and priority
- Create/edit forms with name, description, target type, priority, JSON target config, event type selection
- Detail page with SafePayloadViewer for target config and conditions
- Enable/disable/delete actions with confirmation
- Permission guards on all actions

#### Audit Logs
- Explorer with advanced filters (action, search), hash status badges
- Detail with actor info, entity info, request info, hash chain display
- Before/after snapshots via SafePayloadViewer (redacted)
- Verify hash chain action (if API exists)
- **Read-only**: no edit/delete UI anywhere
- Null and missing fields handled gracefully

#### System Health
- Health dashboard with cards for Payment API, Liveness, Readiness
- Dependency health table with response time and status icons
- Dispatcher/workers status with pending/processing counts
- Gateway provider health summary (links to providers page)
- Failed health sections don't crash the page
- Refresh all button

#### Shared Components Added
- `PageHeader` — Reusable page header with title, back link, and action buttons
- `DetailCard` — Reusable detail section card with title and content
- `Input` — Reusable UI input component
- Enhanced `StatusBadge` with sub-badges: `OutboxStatusBadge`, `WebhookStatusBadge`, `WebhookDeliveryStatusBadge`, `EventTargetTypeBadge`, `AuditHashStatusBadge`, `HealthStatusBadge`, `DispatcherStatusBadge`
- `SafePayloadViewer` — Redacts sensitive fields (secret, token, card, IBAN, bank, account)

#### API Modules
- `events` — Events overview
- `outbox` — Outbox list/detail/retry/skip/dead-letter
- `webhooks` — Webhook CRUD, enable/disable/delete/regenerate-secret
- `webhookDeliveries` — Delivery list/detail/retry/dead-letter
- `eventRouting` — Routing rules CRUD, enable/disable/delete
- `audit` — Audit list/detail/verify-hash-chain
- `systemHealth` — Health/readiness/liveness/dependency/dispatcher endpoints

#### Security
- Webhook secrets never displayed in list/detail views
- New/regenerated secrets shown only once in modal with copy button and warning
- Payloads redacted via SafePayloadViewer (secret, token, card, IBAN, bank fields)
- Delivery headers redacted; signature/secret headers hidden
- All operational actions require confirmation
- Skip/dead-letter actions require reason
- Permission guards on routes and all action buttons
- Protected APIs not called if permission missing

#### API Gaps Documented
- Missing overview API for events dashboard → uses list APIs as fallback
- Missing delivery attempts in outbox detail (optional section)
- Missing dispatcher status in events overview (optional section)
- Health/liveness/readiness endpoints assumed at `/health`, `/health/live`, `/health/ready`

---

## Phase 7.8 — Polish & Production

**Objective:** Hardening, testing, and production readiness.

### Tasks
- Permission hardening across all pages and actions
- Responsive polish (mobile sidebar, layouts)
- RTL/dark mode if required
- Comprehensive test coverage
- Lint/build/test final cleanup
- Final API gaps documentation
- Production Docker deployment config

---

> Last updated: 2026-07-08
