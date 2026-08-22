# Permissions System

> Last updated: 2026-07-08

## Architecture

```
Auth Store (zustand persist)
  └─ user: { id, email, roles: string[], permissions: string[] }
     └─ PermissionProvider (React Context)
        ├─ usePermission() hook
        │   ├─ can(permission, mode?)     → boolean
        │   ├─ canAll(permissions)        → boolean (allOf)
        │   ├─ canAny(permissions)        → boolean (anyOf)
        │   ├─ isAdmin()                  → boolean
        │   ├─ user                       → UserInfo | null
        │   ├─ isAuthenticated            → boolean
        │   └─ permissions                → string[]
        ├─ PermissionGuard (component)
        │   ├─ permission: string | string[]
        │   ├─ mode?: 'exact' | 'anyOf' | 'allOf'
        │   ├─ children: ReactNode
        │   └─ fallback?: ReactNode
        ├─ RoutePermissionGuard (component)
        │   └─ permissions: string[] — ALL required
        └─ ActionPermissionGuard (alias for PermissionGuard)
```

## Permission Strings

### Wallets

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.wallet.view_admin` | `/admin/wallets`, `/admin/wallets/:id`, `/admin/ledger` | View wallets and balances |
| `payment.wallet.adjust_admin` | Wallet detail | Credit/debit wallet |
| `payment.wallet.consistency.manage_admin` | Wallet consistency page | Run wallet/deep checks |

### Payment Intents

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.intent.view_admin` | `/admin/payments`, `/admin/payment-transactions` | View intents & transactions |

### Gateways

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.gateway.provider.view_admin` | `/admin/gateways/providers` | View providers |
| `payment.gateway.config.view` | `/admin/gateways`, `/admin/gateways/configs` | View configs |
| `payment.gateway.config.create` | Config new page | Create config |
| `payment.gateway.config.update` | Config edit page | Update config |
| `payment.gateway.config.delete` | Config detail | Delete config |
| `payment.gateway.config.test` | Config detail | Test gateway |
| `payment.gateway.routing.view_admin` | `/admin/gateways/routing-policies` | View routing |
| `payment.gateway.routing.manage` | Routing create/edit | Manage routing |

### Payment Links

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.payment_link.view_admin` | `/admin/payment-links` | View payment links |

### Payout Accounts

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.payout_account.view_admin` | `/admin/payout-accounts` | View payout accounts |
| `payment.payout_account.verify_admin` | Payout detail/list | Verify account |
| `payment.payout_account.reject_admin` | Payout detail/list | Reject account |
| `payment.payout_account.disable_admin` | Payout detail/list | Disable account |

### Withdrawals

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.withdrawal.view_admin` | `/admin/withdrawals`, `/admin/withdrawals/queue` | View withdrawal queue |
| `payment.withdrawal.approve` | Withdrawal detail | Approve withdrawal |
| `payment.withdrawal.reject` | Withdrawal detail | Reject withdrawal |
| `payment.withdrawal.request_more_info` | Withdrawal detail | Request more info |
| `payment.withdrawal.mark_processing` | Withdrawal detail | Mark as processing |
| `payment.withdrawal.mark_paid` | Withdrawal detail | Mark as paid |
| `payment.withdrawal.mark_failed` | Withdrawal detail | Mark as failed |

### Security / Reports

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.security.reports.view_admin` | `/admin/security`, security pages, reports | View security reports |

### Reconciliation

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.reconciliation.view` | `/admin/security/reconciliation` | View reconciliations |
| `payment.reconciliation.run` | Reconciliation list | Run batch |
| `payment.reconciliation.resolve` | Reconciliation items | Resolve item |
| `payment.reconciliation.false_positive` | Reconciliation items | Mark false positive |

### Risk

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.risk.view` | `/admin/security/risk` | View risk cases/evaluations |
| `payment.risk.manage` | Risk case detail | Acknowledge case |
| `payment.risk.escalate` | Risk case detail | Escalate case |
| `payment.risk.resolve` | Risk case detail | Resolve/close case |
| `payment.risk.false_positive` | Risk case detail | Mark false positive |

### Admin Approvals

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.admin_approval.view` | `/admin/approvals` | View approval requests |
| `payment.admin_approval.approve` | Approval detail | Approve request |
| `payment.admin_approval.reject` | Approval detail | Reject request |
| `payment.admin_approval.execute` | Approval detail | Execute approved request |
| `payment.admin_approval.cancel` | Approval detail | Cancel request |

### Approval Policies

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.admin_approval.policy.view` | `/admin/approval-policies` | View policies |
| `payment.admin_approval.policy.create` | Policy new page | Create policy |
| `payment.admin_approval.policy.update` | Policy edit page | Update policy |
| `payment.admin_approval.policy.delete` | Policy detail | Delete policy |

### Outbox

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.outbox.view_admin` | `/admin/events/outbox` | View outbox events |
| `payment.outbox.retry` | Outbox list/detail | Retry event |
| `payment.outbox.skip` | Outbox list/detail | Skip event |
| `payment.outbox.dead_letter` | Outbox list/detail | Dead-letter event |

### Webhooks

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.webhook.view_admin` | `/admin/events/webhooks`, routing | View subscriptions/routing |
| `payment.webhook.create` | Webhook new page | Create subscription |
| `payment.webhook.update` | Webhook edit page | Update subscription |
| `payment.webhook.delete` | Webhook detail | Delete subscription |
| `payment.webhook.secret.regenerate` | Webhook detail | Regenerate secret |

### Webhook Deliveries

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.webhook.delivery.view_admin` | `/admin/events/webhook-deliveries` | View deliveries |
| `payment.webhook.delivery.retry` | Delivery list/detail | Retry delivery |
| `payment.webhook.delivery.dead_letter` | Delivery list/detail | Dead-letter delivery |

### Event Routing

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.event_routing.view_admin` | `/admin/events/routing` | View routing rules |
| `payment.event_routing.create` | Routing new page | Create rule |
| `payment.event_routing.update` | Routing edit page | Update rule |
| `payment.event_routing.delete` | Routing detail | Delete rule |

### Audit

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.audit.view_admin` | `/admin/audit` | View audit logs |
| `payment.audit.verify_hash_admin` | Audit detail | Verify hash chain |

### System Health

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.system_health.view_admin` | `/admin/system-health` | View system health |

### General

| Permission | Route/Component | Action |
|-----------|----------------|--------|
| `payment.events.view_admin` | `/admin/events` | View events overview |
| `payment.limit_usage.manage_admin` | Limit usage page | Manage limits |
| `payment.ledger.verify_admin` | Ledger integrity page | Verify ledger |

## Super Admin Bypass

Any user with the `super_admin` role bypasses all permission checks and sees all UI.

## Self-Approval Prevention

The admin approval workflow prevents the requester from approving their own
request when configured via:

- **Backend-enforced rule** — The payment backend rejects approvals where the
  approver's user ID matches the requester's user ID.
- **Policy override** — Approval policies can require a different user type.

This is documented in `docs/operation-safety.md`.

## PermissionGuard Modes

| Mode | Behavior |
|------|----------|
| `exact` | User must have the single specified permission |
| `anyOf` | User must have at least one of the listed permissions |
| `allOf` | User must have all of the listed permissions |

`RoutePermissionGuard` uses `allOf` internally.

## Implementation Notes

- Route guards use `RoutePermissionGuard` — renders "Access Denied" or "Please log in"
- Action guards use `PermissionGuard` — hides the action entirely (no fallback shown)
- Sidebar items filter based on permission — hidden if user lacks the permission
- API calls are not made if the user lacks the required permission (guarded UI prevents rendering)
- See `docs/security-review.md` for the full security audit
- See `docs/operation-safety.md` for action confirmation patterns

---

> This document is part of Phase 7.8 final polish documentation.
