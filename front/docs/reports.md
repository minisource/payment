# Reports — Phase 7.2

## Landing Page (`/admin/reports`)

Displays 7 report category cards with links to individual report shells:

| Report | Route | Permission |
|--------|-------|------------|
| Financial Overview | `/admin/reports/financial-overview` | `payment.security.reports.view_admin` |
| Wallets | `/admin/reports/wallets` | `payment.wallet.view_admin` |
| Payments | `/admin/reports/payments` | `payment.intent.view_admin` |
| Gateways | `/admin/reports/gateways` | `payment.gateway.config.view` |
| Withdrawals | `/admin/reports/withdrawals` | `payment.withdrawal.view_admin` |
| Security | `/admin/reports/security` | `payment.security.reports.view_admin` |
| Events | `/admin/reports/events` | `payment.outbox.view_admin` |

## Report Shell

Each individual report page uses `ReportShell`, which provides:
- Title and description
- Required permission badge
- `FilterBar` with `SearchInput`, `DateRangeFilter`, and disabled `ExportButton`
- Placeholder content ("Coming in a later phase")

Full report implementations (filters, charts, data tables, export) are deferred to later phases pending backend API availability.
