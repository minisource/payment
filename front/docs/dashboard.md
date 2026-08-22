# Dashboard — Phase 7.2

The admin dashboard at `/dashboard` provides a comprehensive financial operations overview.

## Structure

| Section | Description | Permission |
|---------|-------------|------------|
| **Header** | Page title, tenant context, date range filter, refresh button | — |
| **Financial Metrics** | Wallet liability by currency (available/locked/pending) | `payment.security.reports.view_admin` |
| **Payment Metrics** | Successful/failed today, success rate, volume | `payment.intent.view_admin` |
| **Withdrawal Metrics** | Pending count/amount, approved count, paid amount | `payment.withdrawal.view_admin` |
| **Gateway Metrics** | Success rate, failed transactions, top provider | `payment.gateway.config.view` |
| **Security & Risk** | Risk cases, reconciliation items, wallet issues, pending approvals | `payment.risk.view` / `payment.reconciliation.view` / `payment.admin_approval.view` |
| **Event Delivery** | Pending/failed/dead-lettered outbox, failed webhooks | `payment.outbox.view_admin` |
| **Charts** | Payment volume (bar), withdrawal volume (line), gateway status (pie), risk severity (pie), event delivery (bar) | Per-widget permissions |
| **Latest Activity** | Compact tables: payment intents, withdrawals, risk cases, reconciliation items, failed deliveries | Per-widget permissions |
| **Quick Navigation** | 8 category link cards | Per-card permissions |

## API Dependencies

All dashboard data comes from the `api/dashboard.ts` module, which calls endpoints under `/api/v1/admin/dashboard/*`. See `docs/api-gaps.md` for missing endpoints.

## State Management

- Each widget/section independently fetches data via `@tanstack/react-query`
- If one API fails, other sections continue to render
- Date range filter passes `dateFrom`/`dateTo` params to all queries
- Refresh button triggers a batch refetch of all queries
