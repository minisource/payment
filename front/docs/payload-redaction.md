# Payload Redaction

> Last updated: 2026-07-08

## Overview

The Payment Admin Frontend uses automatic payload redaction to prevent accidental
exposure of sensitive data in JSON payloads displayed in the UI.

## SafePayloadViewer Component

**Path:** `src/components/shared/safe-payload-viewer.tsx`

A collapsible JSON viewer that automatically redacts sensitive fields.

### Behavior

1. **Collapsed by default** — Payload section headers show only the title
2. **Click to expand** — User must explicitly click the title to reveal content
3. **Auto-redaction** — Sensitive fields are redacted by default
4. **Toggle button** — Authorized users can click the eye icon to toggle redaction
5. **Copy button** — Raw (unredacted) JSON is copied to clipboard

### Redaction Logic

**Sensitive keys** (case-insensitive partial match):

```
secret, token, password, api_key, apiKey, authorization,
card_number, cardNumber, iban, cvv, pan
```

**Sensitive prefixes** (key starts with):

```
card, iban, account_number, private_
```

**Redaction display:**

- String values > 6 chars: first 2 + `••••` + last 2 characters
- String values <= 6 chars: `••••` entirely
- Non-string values: `••••`

### Usage

```tsx
// Simple usage
<SafePayloadViewer data={someObject} />

// With custom title
<SafePayloadViewer data={metadata} title="Request Payload" />

// Null-safe (shows "No data available")
<SafePayloadViewer data={null} />
```

## Pages Using Redaction

| Page | What Gets Redacted |
|------|-------------------|
| Gateway config detail | Config values containing secrets/api keys |
| Payment transaction detail | Callback payload, verify payload |
| Payment link detail | Metadata |
| Outbox event detail | Event payload |
| Webhook delivery detail | Request payload, response body |
| Event routing detail | Target config, conditions |
| Audit log detail | Before/after snapshots, metadata |
| Risk evaluation detail | Input context |
| Risk case detail | Metadata |
| Reconciliation batch detail | Run parameters |
| Reconciliation item detail | Expected/actual values |
| Approval request detail | Execution result, request payload |
| Ledger entry detail | Metadata |
| Withdrawal detail | Bank response |

## Additional Redaction Components

| Component | Usage |
|-----------|-------|
| `MaskedCardNumber` | `**** **** 1234` format for card numbers |
| `MaskedIban` | `IR••••5678` format for IBANs |
| `MaskedAccountNumber` | `••••5678` format for bank accounts |
| `SecretInput` | Form field for secrets with reveal/hide toggle |

## Audit Trail

- Redaction is applied to audit log snapshots (before/after values)
- Audit hash chain status is displayed but hash values are not secrets
- Audit logs are read-only and never expose sensitive payload data

## Testing Redaction

Tests in `src/__tests__/phase-7-7.test.tsx` verify that:
- SafePayloadViewer renders with empty data without crashing
- Null data shows "No data available"
- Sensitive field values (secret, token, card, IBAN) are redacted
- Safe field values remain visible

---

> This document is part of Phase 7.8 final polish documentation.
