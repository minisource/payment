# Secret Handling

> Last updated: 2026-07-08

## Principles

1. **Never display existing secrets.** Secrets that are stored in the backend must
   never be returned to the frontend in plain text. The frontend only shows whether
   the secret is configured or not.
2. **One-time display only.** Newly created or regenerated secrets may be displayed
   once immediately after creation, with an explicit warning that they won't be
   shown again.
3. **No persistence in UI.** Secrets shown in one-time modals are held in React
   component state only. They are never stored in:
   - localStorage
   - URL params
   - Route state
   - Query cache
4. **Copy with warning.** The one-time secret modal includes a copy button but also
   shows a clear warning to store the secret securely.

## Webhook Secrets

### Create Flow

1. User creates a webhook subscription via the form
2. Backend creates the subscription and optionally returns a `secret` field
3. If `secret` is returned, a modal overlay appears with:
   - Warning: "This secret will not be shown again"
   - Secret value in a code block
   - Copy button
   - "Go to Webhook" button that navigates away and dismisses the modal
4. If no `secret` is returned, the user is redirected to the detail page

### Detail View

- The detail page shows only `secretConfigured: boolean`
- Display: `Configured` (green) or `Not configured` (red)
- The actual secret value is never fetched or displayed

### Regenerate Flow

1. User clicks "Regenerate Secret" on the detail page
2. Confirmation dialog appears: "This will generate a new secret. The old secret
   may stop working depending on backend behavior. The new secret will only be
   shown once."
3. After confirmation, a one-time modal appears with:
   - Warning text
   - New secret value
   - Copy button
   - "I've saved the secret" button to dismiss
4. Modal state is held in local React state
5. Closing/dismissing the modal removes the secret from memory

### Edit Form

- The edit form shows a yellow info box:
  "Existing webhook secret is not displayed. Use Regenerate Secret on the
  detail page if needed."
- No secret field exists on the edit form

## Gateway Secrets

### Create Flow

- `SecretInput` component with password field
- Warning: "Secret will be encrypted. Save it securely — it cannot be viewed later."

### Edit Flow

- `SecretInput` with `existing: true` prop
- Shows "configured" badge
- Placeholder: "Leave blank to keep unchanged"
- Help text: "Blank = no change to existing secret"
- Existing secret value is never displayed

### Detail View

- Config values shown via `SafePayloadViewer` which redacts sensitive keys
- Secrets are never displayed

## Redaction Components

| Component | Purpose |
|-----------|---------|
| `SecretInput` | Form input for secrets with reveal/hide toggle |
| `MaskedCardNumber` | Masked card number display (`**** **** 1234`) |
| `MaskedIban` | Masked IBAN display (`IR••••5678`) |
| `MaskedAccountNumber` | Masked account number display (`••••5678`) |
| `SafePayloadViewer` | JSON viewer with automatic field-level redaction |

---

> This document is part of Phase 7.8 final polish documentation.
