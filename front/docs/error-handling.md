# Error Handling

> Last updated: 2026-07-08

## Overview

Standardized error handling across the Payment Admin Frontend.

## Backend Error Shape

The payment backend returns errors in this standard format:

```json
{
  "error": {
    "code": "payment_forbidden",
    "message": "Forbidden",
    "user_message": "شما دسترسی لازم را ندارید.",
    "category": "permission",
    "details": {},
    "field_errors": [
      { "field": "name", "message": "Name is required" }
    ],
    "request_id": "req_abc123",
    "correlation_id": "corr_def456"
  }
}
```

## ApiError Class

**Path:** `src/api/client.ts`

```typescript
class ApiError extends Error {
  status: number;           // HTTP status code
  code: string;             // Backend error code (e.g. 'payment_forbidden')
  message: string;          // Technical error message
  userMessage?: string;     // User-facing message
  category?: string;        // validation | business | permission | not_found | system
  fieldErrors?: Array<{ field: string; message: string }>;
  requestId?: string;
  correlationId?: string;
}
```

## Error Categories

| Category | HTTP Status | Example | Frontend Behavior |
|----------|-------------|---------|-------------------|
| `validation` | 400 | Missing required field | Field-level errors under inputs |
| `business` | 422 | Insufficient balance | Alert/toast with user_message |
| `permission` | 403 | Forbidden | Route guard shows "Access Denied"; action guard hides button |
| `not_found` | 404 | Entity not found | Page-level error state |
| `system` | 500 | Internal error | Toast with request_id |

## Error Display Patterns

### Page-Level Errors

```tsx
// Main entity load failure
if (isError || !data) {
  return <ErrorState error={(error as Error)?.message} onRetry={() => refetch()} />;
}
```

### Section-Level Errors

```tsx
// Independent section failure — does not crash the page
<Card className="border-red-200 bg-red-50">
  <CardContent className="p-4">
    <p className="text-sm text-red-700">Section unavailable: {errorMessage}</p>
  </CardContent>
</Card>
```

### Field-Level Errors

```tsx
// Form validation errors
{fieldError && <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">{fieldError}</div>}
```

### Toast Errors

```tsx
// API action failures
try {
  await someAction();
  toast.success('Action completed');
} catch (err: any) {
  toast.error(err.userMessage || err.message || 'Action failed');
}
```

### Request ID Display

```tsx
// Expandable debug section for API errors
<details className="text-xs text-muted-foreground">
  <summary>Debug Info</summary>
  <p>Request ID: {requestId}</p>
  <p>Correlation ID: {correlationId}</p>
</details>
```

## HTTP Status Handling

| Status | Frontend Action |
|--------|----------------|
| 200-299 | Success — normal flow |
| 400 | Show field_errors or business error alert |
| 401 | Clear auth store → redirect to login |
| 403 | RoutePermissionGuard shows "Access Denied" |
| 404 | Page shows ErrorState "Not found" |
| 422 | Business error with user_message |
| 429 | Rate limit — show toast |
| 500+ | System error with request_id |
| Network error | Toast "Network request failed" |

## Error Components

| Component | Path | Purpose |
|-----------|------|---------|
| `ErrorState` | `components/shared/states.tsx` | Centered error with retry button |
| `Loading` | `components/shared/states.tsx` | Centered spinner |
| `EmptyState` | `components/shared/states.tsx` | Centered empty message |
| `toast.success()` | `sonner` | Success notification |
| `toast.error()` | `sonner` | Error notification |
| `RoutePermissionGuard` | `components/shared/permission-guards.tsx` | Auth/permission error state |
| `PermissionGuard` | `components/shared/permission-guards.tsx` | Action hidden state |

## Error Recovery

- **Retry**: All error states have a retry button that re-fetches the data
- **Navigation**: Detail pages have back links to parent list pages
- **Refresh**: List pages have a refresh button
- **No crash**: Section failures do not crash the entire page

---

> This document is part of Phase 7.8 final polish documentation.
