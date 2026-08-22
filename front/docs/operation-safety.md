# Operation Safety

> Last updated: 2026-07-08

## Overview

This document describes the safety patterns used for financial and operational
actions in the Payment Admin Frontend.

## Safety Principles

1. **No optimistic updates.** UI state is only updated after backend API succeeds.
   No fake "success" state is shown before the API confirms.
2. **Confirmation required.** Every sensitive action requires user confirmation
   via a dialog.
3. **Reason required for destructive actions.** Skip, dead-letter, reject, and
   other destructive or irreversible actions require an explicit reason.
4. **Strong confirmation for dangerous actions.** Actions that affect financial
   state or data permanence use strong confirmation language.
5. **Permission guards.** Every actionable button is wrapped in `PermissionGuard`.

## Confirmation Levels

### Level 1: Simple Confirmation

Used for reversible or low-risk actions.

```
Dialog: "Are you sure?"
Buttons: Confirm / Cancel
```

Examples:
- Enable/disable webhook subscription
- Enable/disable event routing rule

### Level 2: Confirmation with Reason

Used for actions that change processing flow or where audit trail is important.

```
Dialog: "Action requires a reason."
        [Reason input field - required]
Buttons: Confirm (disabled without reason) / Cancel
```

Examples:
- Skip outbox event
- Dead-letter outbox event
- Reject withdrawal
- Mark withdrawal as failed
- Dead-letter webhook delivery

### Level 3: Strong Confirmation

Used for irreversible or financially impactful actions.

```
Dialog: "This action is irreversible/will change financial state. Are you sure?"
Buttons: Execute (danger styling) / Cancel
```

Examples:
- Delete webhook subscription
- Regenerate webhook secret
- Delete gateway config
- Execute approved approval request
- Mark withdrawal as paid
- Wallet credit/debit adjustment

## Confirmation Dialog Component

**Path:** `src/components/shared/components.tsx` (ConfirmDialog)

```tsx
<ConfirmDialog
  open={isOpen}
  onClose={handleClose}
  onConfirm={handleConfirm}
  title="Action Title"
  description={<p>Description text</p>}
  confirmLabel="Execute"
  confirmDisabled={!reason}
  destructive={true}
/>
```

Props:
| Prop | Type | Description |
|------|------|-------------|
| `open` | boolean | Dialog visibility |
| `onClose` | function | Close handler |
| `onConfirm` | function | Confirm handler |
| `title` | string | Dialog title |
| `description` | string/ReactNode | Dialog content |
| `confirmLabel` | string | Confirm button text |
| `confirmDisabled` | boolean | Disable confirm button |
| `destructive` | boolean | Show confirm button in danger color |

## Action Flow Pattern

Every operational action follows this pattern:

1. User clicks action button
2. Confirmation dialog appears (level 1/2/3 as appropriate)
3. User confirms (optionally fills reason)
4. API call is made
5. On success: toast notification + data refresh
6. On failure: toast with error (including request_id/correlation_id)
7. UI is NOT optimistically updated

## Financial Action Review

| Action | Level | Reason | Permission |
|--------|-------|--------|------------|
| Wallet credit | 3 | Yes | `payment.wallet.adjust_admin` |
| Wallet debit | 3 | Yes | `payment.wallet.adjust_admin` |
| Withdrawal approve | 1 | No | `payment.withdrawal.approve` |
| Withdrawal reject | 2 | Yes | `payment.withdrawal.reject` |
| Withdrawal mark paid | 3 | No | `payment.withdrawal.mark_paid` |
| Withdrawal mark failed | 3 | Yes | `payment.withdrawal.mark_failed` |
| Reconciliation run | 1 | No | `payment.reconciliation.run` |
| Reconciliation resolve | 1 | No | `payment.reconciliation.resolve` |
| Reconciliation false-positive | 1 | No | `payment.reconciliation.false_positive` |
| Approval execute | 3 | No | `payment.admin_approval.execute` |

## Operational Action Review

| Action | Level | Reason | Permission |
|--------|-------|--------|------------|
| Outbox retry | 1 | No | `payment.outbox.retry` |
| Outbox skip | 2 | Yes | `payment.outbox.skip` |
| Outbox dead-letter | 3 | Yes | `payment.outbox.dead_letter` |
| Webhook enable | 1 | No | `payment.webhook.update` |
| Webhook disable | 1 | No | `payment.webhook.update` |
| Webhook delete | 3 | No | `payment.webhook.delete` |
| Webhook regenerate secret | 3 | No | `payment.webhook.secret.regenerate` |
| Webhook delivery retry | 1 | No | `payment.webhook.delivery.retry` |
| Webhook delivery dead-letter | 2 | Yes | `payment.webhook.delivery.dead_letter` |
| Event routing enable | 1 | No | `payment.event_routing.update` |
| Event routing disable | 1 | No | `payment.event_routing.update` |
| Event routing delete | 1 | No | `payment.event_routing.delete` |
| Gateway config enable/disable | 1 | No | `payment.gateway.config.update` |
| Gateway config delete | 3 | No | `payment.gateway.config.delete` |

## Error Handling After Action

All action mutation errors should:
1. Display the error in a toast notification
2. Show `request_id` / `correlation_id` if available
3. NOT update UI state since the API call failed

## Testing Safety

Tests in `src/__tests__/phase-7-7.test.tsx` verify:
- Retry/skip/dead-letter actions require confirmation
- Skip/dead-letter require reason
- Webhook secret actions are permission-guarded
- Audit logs have no edit/delete UI

---

> This document is part of Phase 7.8 final polish documentation.
