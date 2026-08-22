# Security Review

> Last updated: 2026-07-08

## Overview

This document summarizes the security review of the Payment Admin Frontend, covering
secret exposure, payload redaction, permission checks, and financial operation safety.

## Secret Exposure Audit

| Secret Type | Exposure Risk | Mitigation |
|-------------|---------------|------------|
| Gateway secrets | Never displayed in UI | `SecretInput` with placeholder text "Leave blank to keep unchanged" |
| Webhook secrets (existing) | Never displayed | Detail page shows only `secretConfigured: boolean` |
| Webhook secrets (new) | Shown once after create/regenerate | One-time modal with copy button + warning "will not be shown again" |
| Auth tokens | Stored in localStorage | Used only for API Authorization header |
| API keys | Never displayed | Redacted by `SafePayloadViewer` |
| Card numbers | Masked | `MaskedCardNumber` shows `**** **** 1234` format |
| IBAN numbers | Masked | `MaskedIban` shows `IR••••5678` format |
| Bank accounts | Masked | `MaskedAccountNumber` shows `••••5678` format |

## Payload Redaction

The `SafePayloadViewer` component automatically redacts sensitive fields from
any JSON payload displayed in the UI.

### Redacted Keys

The following key patterns are detected and redacted (case-insensitive, partial match):

```
secret, token, password, api_key, apiKey, authorization,
card_number, cardNumber, iban, cvv, pan
```

### Redacted Prefixes

Keys starting with these prefixes are also redacted:

```
card, iban, account_number, private_
```

### Redaction Behavior

- Strings longer than 6 chars show first 2 + `••••` + last 2 chars
- Strings shorter than 6 chars show `••••`
- Non-string values show `••••`
- Toggle button allows authorized users to view redacted values

### Payload Viewers Using Redaction

All these pages use `SafePayloadViewer`:

- Gateway config detail (config values)
- Payment transaction detail (callback, verify payloads)
- Payment link detail (metadata)
- Outbox event detail (event payload)
- Webhook delivery detail (request payload, response body)
- Event routing rule detail (target config, conditions)
- Audit log detail (before/after snapshots, metadata)
- Risk evaluation detail (input context)
- Risk case detail (metadata)
- Reconciliation batch detail (run parameters)
- Reconciliation item detail (expected/actual values)
- Approval request detail (execution result, request payload)
- Ledger entry detail (metadata)
- Withdrawal detail (bank response)

## Permission Hardening

### Route-Level Guards

Every admin page is wrapped in `RoutePermissionGuard`. See `docs/permissions.md`
for the full permission-to-route mapping.

Key rules:
- `super_admin` role bypasses all permission checks
- Missing permissions show "Access Denied" page
- Not logged in shows "Please log in" page
- Sidebar items are filtered by permission

### Action-Level Guards

Sensitive actions use `PermissionGuard`:

| Page | Protected Actions |
|------|-------------------|
| Wallet detail | Adjust credit/debit |
| Gateway config | Create, update, delete, test |
| Gateway routing | Create, edit, delete |
| Payout accounts | Verify, reject, disable |
| Withdrawals | Approve, reject, mark-paid, mark-failed |
| Reconciliation | Run batch, resolve item, false-positive |
| Risk cases | Acknowledge, escalate, resolve, close |
| Admin approvals | Approve, reject, execute, cancel |
| Outbox events | Retry, skip, dead-letter |
| Webhook subscriptions | Create, update, delete, regenerate secret |
| Webhook deliveries | Retry, dead-letter |
| Event routing | Create, update, delete |

## Financial Operation Safety

| Operation | Confirmation | Reason Required | Permission |
|-----------|-------------|-----------------|------------|
| Wallet credit/debit | ✅ Yes | ✅ Yes | `payment.wallet.adjust_admin` |
| Withdrawal approve | ✅ Yes | ❌ Optional | `payment.withdrawal.approve` |
| Withdrawal reject | ✅ Yes | ✅ Yes | `payment.withdrawal.reject` |
| Withdrawal mark paid | ✅ Yes (strong) | ❌ Optional | `payment.withdrawal.mark_paid` |
| Withdrawal mark failed | ✅ Yes (strong) | ✅ Yes | `payment.withdrawal.mark_failed` |
| Outbox retry | ✅ Yes | ❌ Optional | `payment.outbox.retry` |
| Outbox skip | ✅ Yes | ✅ Yes | `payment.outbox.skip` |
| Outbox dead-letter | ✅ Yes (strong) | ✅ Yes | `payment.outbox.dead_letter` |
| Webhook delete | ✅ Yes (strong) | ❌ No | `payment.webhook.delete` |
| Webhook regenerate secret | ✅ Yes (strong) | ❌ No | `payment.webhook.secret.regenerate` |
| Gateway config delete | ✅ Yes (strong) | ❌ No | `payment.gateway.config.delete` |
| Reconciliation run | ✅ Yes | ❌ No | `payment.reconciliation.run` |
| Approval execute | ✅ Yes (strong) | ❌ No | `payment.admin_approval.execute` |

## Known Security Limitations

1. **Auth token in localStorage**: Tokens are stored in localStorage which is accessible to any JavaScript running on the same origin. This is mitigated by short token expiry and HTTPS-only deployment.
2. **CSRF**: The API uses `Authorization: Bearer` headers, not cookies. Bearer tokens in headers are inherently immune to standard CSRF attacks — no CSRF token is needed.
3. **Dev mock auth**: `NEXT_PUBLIC_DEV_MOCK_AUTH=true` bypasses all auth. Never set in production.
4. **Debug panel**: No debug panel is implemented and none should leak request/response data.

## Self-Approval Prevention

The admin approval system prevents the requester from approving their own request
when configured via approval policies (backend-enforced rule).

---

> This document is part of Phase 7.8 final polish documentation.
