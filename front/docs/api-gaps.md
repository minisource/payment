# Payment Frontend — API Gaps & Backend TODOs

> Auto-generated during Phase 7 frontend implementation.
> This file documents API endpoints, schemas, or features that are missing or
> unclear from the frontend perspective.

---

## Missing API Endpoints (Phase 7.7 — Outbox, Webhooks, Events, Audit, Health)

### Events Overview

| Feature | Missing Endpoint | Suggested Response Shape | Priority |
|---------|-----------------|--------------------------|----------|
| Events overview summary | `GET /api/v1/admin/events/overview` | `{ pendingOutboxEvents, processingOutboxEvents, failedOutboxEvents, deadLetteredEvents, activeWebhookSubscriptions, failedWebhookDeliveries, averageDeliveryLatencyMs, dispatcherStatus }` | 🟡 Medium |

**Fallback:** Frontend currently shows `—` for missing overview data and uses individual list APIs where practical.

### Outbox

| Feature | Missing Endpoint | Notes |
|---------|-----------------|-------|
| Outbox event retry | `POST /api/v1/admin/outbox/events/{eventId}/retry` | Request body: `{ reason?: string }` |
| Outbox event skip | `POST /api/v1/admin/outbox/events/{eventId}/skip` | Request body: `{ reason: string }` |
| Outbox event dead-letter | `POST /api/v1/admin/outbox/events/{eventId}/dead-letter` | Request body: `{ reason: string }` |

**Frontend behavior:** Retry/skip/dead-letter buttons only appear if endpoints exist. API calls currently use `POST /api/v1/admin/outbox/events/{eventId}/{action}`.

### Webhooks

| Feature | Missing Endpoint | Notes |
|---------|-----------------|-------|
| Webhook subscription enable | `POST /api/v1/admin/webhooks/subscriptions/{id}/enable` | |
| Webhook subscription disable | `POST /api/v1/admin/webhooks/subscriptions/{id}/disable` | |
| Webhook subscription regenerate secret | `POST /api/v1/admin/webhooks/subscriptions/{id}/regenerate-secret` | Returns `{ subscription_id, secret, rotated_at }` |
| Webhook subscription update | `PATCH /api/v1/admin/webhooks/subscriptions/{id}` | Partial update |

**Secret handling:** Backend MUST return the secret ONLY on create and regenerate responses. Existing subscriptions must NEVER expose the secret in GET responses.

### Webhook Deliveries

| Feature | Missing Endpoint | Notes |
|---------|-----------------|-------|
| Webhook delivery retry | `POST /api/v1/admin/webhooks/deliveries/{id}/retry` | |
| Webhook delivery dead-letter | `POST /api/v1/admin/webhooks/deliveries/{id}/dead-letter` | Request body: `{ reason: string }` |

### Audit

| Feature | Missing Endpoint | Notes |
|---------|-----------------|-------|
| Audit hash chain verification | `POST /api/v1/admin/audit/verify-hash-chain` | Request: `{ auditLogId: string }`, Response: `{ valid: boolean; message?: string }` |

### System Health

| Feature | Missing Endpoint | Suggested Response Shape | Priority |
|---------|-----------------|--------------------------|----------|
| System health detail | `GET /api/v1/admin/system-health` | `{ status: string; checkedAt: string; uptime?: string; version?: string }` | 🟡 Medium |
| Dependency health list | `GET /api/v1/admin/system-health/dependencies` | `{ items: [{ name: string; type: string; status: string; responseTimeMs?: number; error?: string }] }` | 🟡 Medium |
| Dispatcher status list | `GET /api/v1/admin/system-health/dispatchers` | `{ items: [{ name: string; type: string; status: string; isRunning: boolean; pendingCount: number; processingCount: number }] }` | 🟡 Medium |
| Liveness check | `GET /health/live` | `{ status: string }` | 🟢 Low |
| Readiness check | `GET /health/ready` | `{ status: string; checks?: [{ name: string; status: string; responseTimeMs?: number }] }` | 🟢 Low |

---

## Missing API Endpoints (Phase 7.2 Dashboard & Reports)

| Feature | Missing Endpoint | Suggested Response Shape | Priority |
|---------|-----------------|--------------------------|----------|
| Dashboard overview | `GET /api/v1/admin/dashboard/overview` | `{ liability: LiabilitySummary[], payments: PaymentMetricSummary[], withdrawals: WithdrawalMetricSummary[], gateways: GatewayMetricSummary, security: SecurityMetricSummary, events: EventDeliveryMetricSummary }` | 🔴 High |
| Payment volume chart | `GET /api/v1/admin/dashboard/charts/payment-volume` | `ChartDataPoint[]` with `{ label, value, date? }` | 🟡 Medium |
| Withdrawal volume chart | `GET /api/v1/admin/dashboard/charts/withdrawal-volume` | `ChartDataPoint[]` with `{ label, value, date? }` | 🟡 Medium |
| Gateway status chart | `GET /api/v1/admin/dashboard/charts/gateway-status` | `ChartDataPoint[]` with `{ label, value }` (success/failure by provider) | 🟡 Medium |
| Risk severity chart | `GET /api/v1/admin/dashboard/charts/risk-severity` | `ChartDataPoint[]` with `{ label, value }` (count by severity) | 🟡 Medium |
| Event delivery chart | `GET /api/v1/admin/dashboard/charts/event-delivery` | `ChartDataPoint[]` with `{ label, value }` (count by status) | 🟡 Medium |
| Latest payment intents | `GET /api/v1/admin/dashboard/latest-payment-intents` | `{ items: LatestPaymentIntentItem[] }` with `limit` param | 🟡 Medium |
| Latest withdrawals | `GET /api/v1/admin/dashboard/latest-withdrawals` | `{ items: LatestWithdrawalItem[] }` with `limit` param | 🟡 Medium |
| Latest risk cases | `GET /api/v1/admin/dashboard/latest-risk-cases` | `{ items: LatestRiskCaseItem[] }` with `limit` param | 🟡 Medium |
| Latest reconciliation items | `GET /api/v1/admin/dashboard/latest-reconciliation-items` | `{ items: LatestReconciliationItem[] }` with `limit` param | 🟡 Medium |
| Latest event deliveries | `GET /api/v1/admin/dashboard/latest-event-deliveries` | `{ items: LatestEventDeliveryItem[] }` with `limit` param | 🟡 Medium |
| Reports overview | `GET /api/v1/admin/reports/overview` | `{ categories: ReportCategory[] }` | 🟢 Low |
| Financial report | `GET /api/v1/admin/reports/financial-overview` | Filterable report data with charts + table | 🟢 Low |
| Wallet report | `GET /api/v1/admin/reports/wallets` | Filterable report data with charts + table | 🟢 Low |
| Payment report | `GET /api/v1/admin/reports/payments` | Filterable report data with charts + table | 🟢 Low |
| Gateway report | `GET /api/v1/admin/reports/gateways` | Filterable report data with charts + table | 🟢 Low |
| Withdrawal report | `GET /api/v1/admin/reports/withdrawals` | Filterable report data with charts + table | 🟢 Low |
| Security report | `GET /api/v1/admin/reports/security` | Filterable report data with charts + table | 🟢 Low |
| Events report | `GET /api/v1/admin/reports/events` | Filterable report data with charts + table | 🟢 Low |

### Phase 7.2 Type Definitions Needed in Backend

```typescript
// Liability by currency
interface LiabilitySummary {
  currency: string;
  availableTotal: number;
  lockedTotal: number;
  pendingTotal: number;
}

// Payment metrics
interface PaymentMetricSummary {
  successfulToday: number;
  failedToday: number;
  successRate: number;       // 0.0–1.0
  totalVolume: number;
  currency: string;
}

// Withdrawal metrics
interface WithdrawalMetricSummary {
  pendingCount: number;
  pendingAmount: number;
  approvedCount: number;
  paidAmount: number;
  currency: string;
}

// Gateway metrics
interface GatewayMetricSummary {
  successRate: number;       // 0.0–1.0
  failedTransactions: number;
  topProvider: string | null;
}

// Security metrics
interface SecurityMetricSummary {
  openRiskCases: number;
  criticalRiskCases: number;
  unresolvedRecItems: number;
  walletConsistencyIssues: number;
  pendingApprovals: number;
}

// Event delivery metrics
interface EventDeliveryMetricSummary {
  pendingOutbox: number;
  failedOutbox: number;
  deadLetteredOutbox: number;
  failedWebhookDeliveries: number;
}
```

---

## Missing API Endpoints (Phase 7.1 — Operational)

| Feature | Missing Endpoint | Notes |
|---------|-----------------|-------|
| Wallet list | `GET /api/v1/admin/wallets` | Expected paginated wallet list with filters (ownerType, currency, status, balance range) |
| Ledger list | `GET /api/v1/admin/ledger` | Global ledger search with filters (walletId, entryType, direction, currency, date range) |
| Payment intents list | `GET /api/v1/admin/payment-intents` | Paginated with filters (status, provider, currency, external_reference) |
| Gateway providers | `GET /api/v1/admin/gateways/providers` | List of enabled gateway providers |
| Gateway configs CRUD | `GET/POST/PUT /api/v1/admin/gateways/configs` | Full CRUD for gateway configurations |
| Payment links admin | `GET /api/v1/admin/payment-links` | Admin list with pause/resume/disable |
| Payout accounts admin | `GET /api/v1/admin/payout-accounts` | Admin list with verify/reject/disable |
| Withdrawal queue | `GET /api/v1/admin/withdrawals` | Paginated with filters + queue view |
| Withdrawal actions | `POST .../approve`, `POST .../reject`, `POST .../mark-paid`, `POST .../mark-failed` | Admin actions on withdrawals |
| Reconciliation | `GET/POST /api/v1/admin/reconciliation/*` | Run batches, list batches, resolve items |
| Admin approval policies | `GET/POST/PUT/DELETE /api/v1/admin/approval-policies` | CRUD for approval policies |
| Event routing rules | `GET/POST/PUT/DELETE /api/v1/admin/events/routing` | CRUD for event routing rules |
| System health | `GET /health` | Health check endpoint |
| System health detail | `GET /api/v1/admin/system-health` | Detailed status (DB, Redis, gateway status) |
| Settings | `GET /api/v1/admin/settings` | Payment + tenant settings |
| Reports | Multiple endpoints under `/api/v1/admin/reports/*` | Financial, wallet, payment, gateway, security reports |

## Unclear Schemas

| Entity | Issue |
|--------|-------|
| GatewayConfig | ConfigJson schema unclear — what fields are expected? |
| ApprovalRequest | Payload schema varies by operationType — need documentation per type |
| ReconciliationItem | Status values unclear (open, resolved, false_positive?) |
| WalletConsistencyCheck | Response structure for deep-check endpoint unknown |

## Missing Features in Backend

| Feature | Notes |
|---------|-------|
| Tenant/application context switching | Frontend needs tenant list API + switch mechanism |
| Permission list API | Frontend needs to know which permissions the current user has |
| Export/CSV | No export endpoints for reports/data |
| Soft-delete for payment links | Pause/resume/disable/delete lifecycle |
| Payout account verification flow | Verify/reject/disable with reason |
| Wallet freeze/disable | Admin ability to freeze a wallet |
| Ledger hash verification badge | Quick check if hash chain is valid for a wallet |

---

> Last updated: 2026-07-08
