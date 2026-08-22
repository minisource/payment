# Routes

> Last updated: 2026-07-08

All admin routes use the `(main)` layout group with shared sidebar, auth context,
and permission guards.

## Route Map

| Route | Page | Phase | Permission |
|-------|------|-------|------------|
| `/` | Redirect to `/dashboard` | 7.1 | — |
| `/dashboard` | Dashboard | 7.2 | — (all authenticated users) |
| `/admin` | Redirect to `/dashboard` | 7.1 | — |
| `/admin/403` | Forbidden | 7.1 | — |
| `/admin/settings` | Settings (placeholder) | 7.8 | `payment.gateway.config.view` |

### Wallets
| Route | Page | Permission |
|-------|------|------------|
| `/admin/wallets` | Wallet List | `payment.wallet.view_admin` |
| `/admin/wallets/:id` | Wallet Detail | `payment.wallet.view_admin` |
| `/admin/ledger` | Ledger Explorer | `payment.wallet.view_admin` |
| `/admin/ledger/:id` | Ledger Entry Detail | `payment.wallet.view_admin` |

### Payments
| Route | Page | Permission |
|-------|------|------------|
| `/admin/payments` | Payment Intents | `payment.intent.view_admin` |
| `/admin/payments/:id` | Payment Intent Detail | `payment.intent.view_admin` |
| `/admin/payment-transactions` | Payment Transactions | `payment.intent.view_admin` |
| `/admin/payment-transactions/:id` | Payment Transaction Detail | `payment.intent.view_admin` |

### Gateways
| Route | Page | Permission |
|-------|------|------------|
| `/admin/gateways` | Gateways Overview | `payment.gateway.config.view` |
| `/admin/gateways/providers` | Provider Catalog | `payment.gateway.provider.view_admin` |
| `/admin/gateways/configs` | Gateway Configs | `payment.gateway.config.view` |
| `/admin/gateways/configs/new` | Create Config | `payment.gateway.config.create` |
| `/admin/gateways/configs/:id` | Config Detail | `payment.gateway.config.view` |
| `/admin/gateways/configs/:id/edit` | Edit Config | `payment.gateway.config.update` |
| `/admin/gateways/routing-policies` | Routing Policies | `payment.gateway.routing.view_admin` |
| `/admin/gateways/routing-policies/new` | Create Policy | `payment.gateway.routing.manage` |
| `/admin/gateways/routing-policies/:id` | Policy Detail | `payment.gateway.routing.view_admin` |
| `/admin/gateways/routing-policies/:id/edit` | Edit Policy | `payment.gateway.routing.manage` |

### Payment Links
| Route | Page | Permission |
|-------|------|------------|
| `/admin/payment-links` | Payment Link List | `payment.payment_link.view_admin` |
| `/admin/payment-links/:id` | Payment Link Detail | `payment.payment_link.view_admin` |

### Payout & Withdrawals
| Route | Page | Permission |
|-------|------|------------|
| `/admin/payout-accounts` | Payout Account List | `payment.payout_account.view_admin` |
| `/admin/payout-accounts/:id` | Payout Account Detail | `payment.payout_account.view_admin` |
| `/admin/withdrawals` | Withdrawal Queue | `payment.withdrawal.view_admin` |
| `/admin/withdrawals/queue` | Withdrawal Queue | `payment.withdrawal.view_admin` |
| `/admin/withdrawals/:id` | Withdrawal Detail | `payment.withdrawal.view_admin` |

### Security
| Route | Page | Permission |
|-------|------|------------|
| `/admin/security` | Security Overview | `payment.security.reports.view_admin` |
| `/admin/security/overview` | Security Overview | `payment.security.reports.view_admin` |
| `/admin/security/ledger-integrity` | Ledger Integrity | `payment.security.reports.view_admin` |
| `/admin/security/wallet-consistency` | Wallet Consistency | `payment.security.reports.view_admin` |
| `/admin/security/limit-usage` | Limit Usage | `payment.security.reports.view_admin` |
| `/admin/security/reconciliation` | Reconciliation Overview | `payment.reconciliation.view` |
| `/admin/security/reconciliation/batches` | Reconciliation Batches | `payment.reconciliation.view` |
| `/admin/security/reconciliation/batches/:id` | Batch Detail | `payment.reconciliation.view` |
| `/admin/security/reconciliation/items` | Reconciliation Items | `payment.reconciliation.view` |
| `/admin/security/reconciliation/items/:id` | Item Detail | `payment.reconciliation.view` |
| `/admin/security/risk/evaluations` | Risk Evaluations | `payment.risk.view` |
| `/admin/security/risk/evaluations/:id` | Evaluation Detail | `payment.risk.view` |
| `/admin/security/risk/cases` | Risk Cases | `payment.risk.view` |
| `/admin/security/risk/cases/:id` | Case Detail | `payment.risk.view` |

### Admin Approvals
| Route | Page | Permission |
|-------|------|------------|
| `/admin/approvals` | Approval Requests | `payment.admin_approval.view` |
| `/admin/approvals/:id` | Approval Detail | `payment.admin_approval.view` |
| `/admin/approval-policies` | Approval Policies | `payment.admin_approval.policy.view` |
| `/admin/approval-policies/new` | Create Policy | `payment.admin_approval.policy.create` |
| `/admin/approval-policies/:id` | Policy Detail | `payment.admin_approval.policy.view` |
| `/admin/approval-policies/:id/edit` | Edit Policy | `payment.admin_approval.policy.update` |

### Events
| Route | Page | Permission |
|-------|------|------------|
| `/admin/events` | Events Overview | `payment.events.view_admin` |
| `/admin/events/outbox` | Outbox List | `payment.outbox.view_admin` |
| `/admin/events/outbox/:id` | Outbox Event Detail | `payment.outbox.view_admin` |
| `/admin/events/webhooks` | Webhook Subscriptions | `payment.webhook.view_admin` |
| `/admin/events/webhooks/new` | Create Webhook | `payment.webhook.create` |
| `/admin/events/webhooks/:id` | Webhook Detail | `payment.webhook.view_admin` |
| `/admin/events/webhooks/:id/edit` | Edit Webhook | `payment.webhook.update` |
| `/admin/events/webhook-deliveries` | Webhook Deliveries | `payment.webhook.delivery.view_admin` |
| `/admin/events/webhook-deliveries/:id` | Delivery Detail | `payment.webhook.delivery.view_admin` |
| `/admin/events/routing` | Event Routing Rules | `payment.event_routing.view_admin` |
| `/admin/events/routing/new` | Create Rule | `payment.event_routing.create` |
| `/admin/events/routing/:id` | Rule Detail | `payment.event_routing.view_admin` |
| `/admin/events/routing/:id/edit` | Edit Rule | `payment.event_routing.update` |

### Route Aliases (Events)
| Route | Redirects To | Permission |
|-------|-------------|------------|
| `/admin/outbox` | `/admin/events/outbox` | `payment.outbox.view_admin` |
| `/admin/outbox/:id` | `/admin/events/outbox/:id` | `payment.outbox.view_admin` |
| `/admin/webhooks` | `/admin/events/webhooks` | `payment.webhook.view_admin` |
| `/admin/webhooks/new` | `/admin/events/webhooks/new` | `payment.webhook.create` |
| `/admin/webhooks/:id` | `/admin/events/webhooks/:id` | `payment.webhook.view_admin` |
| `/admin/webhooks/:id/edit` | `/admin/events/webhooks/:id/edit` | `payment.webhook.update` |

### Audit
| Route | Page | Permission |
|-------|------|------------|
| `/admin/audit` | Audit Logs Explorer | `payment.audit.view_admin` |
| `/admin/audit/:id` | Audit Log Detail | `payment.audit.view_admin` |

### Reports
| Route | Page | Permission |
|-------|------|------------|
| `/admin/reports` | Reports Shell | `payment.security.reports.view_admin` |
| `/admin/reports/financial-overview` | Financial Report | `payment.security.reports.view_admin` |
| `/admin/reports/wallets` | Wallet Report | `payment.security.reports.view_admin` |
| `/admin/reports/payments` | Payment Report | `payment.security.reports.view_admin` |
| `/admin/reports/gateways` | Gateway Report | `payment.security.reports.view_admin` |
| `/admin/reports/withdrawals` | Withdrawal Report | `payment.security.reports.view_admin` |
| `/admin/reports/security` | Security Report | `payment.security.reports.view_admin` |
| `/admin/reports/events` | Events Report | `payment.security.reports.view_admin` |

### System
| Route | Page | Permission |
|-------|------|------------|
| `/admin/system-health` | System Health | `payment.system_health.view_admin` |

## Route Protection

- All `/admin/*` routes require authentication
- Each route has a specific permission via `RoutePermissionGuard`
- Missing permission → "Access Denied" page
- Not authenticated → "Please log in" page
- Unknown routes → 404 page
- `/admin/403` → Forbidden page (rendered in layout)

---

> This document is part of Phase 7.8 final polish documentation.
