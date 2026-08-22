# Testing

> Last updated: 2026-07-08

## Test Setup

- **Framework:** Vitest
- **Rendering:** @testing-library/react
- **DOM:** jsdom
- **Matchers:** @testing-library/jest-dom

**Test configuration:** `vitest.config.ts`

**Setup file:** `src/__tests__/setup.ts` — imports jest-dom matchers, mocks `window.matchMedia`

## Running Tests

```bash
# Run all tests
npm run test

# Watch mode
npm run test -- --watch

# Specific file
npm run test -- src/__tests__/some-test.test.tsx

# Coverage
npm run test:coverage
```

## Test Files

| File | Tests | Focus |
|------|-------|-------|
| `app.test.tsx` | 2 | Root layout renders, providers work |
| `auth-store.test.ts` | 6 | Zustand auth store: setAuth, clearAuth, hasRole, hasPermission, persistence |
| `api-client.test.ts` | 6 | API client: base URL, header injection, error parsing |
| `permission-guard.test.tsx` | 4 | PermissionGuard: exact mode, missing permission, anyOf mode |
| `shared-components.test.tsx` | 5 | MoneyAmount, StatusBadge, CopyText components |
| `status-badge.test.tsx` | 7 | StatusBadge: known/unknown statuses, color classes, RiskSeverityBadge |
| `metric-card.test.tsx` | 3 | MetricCard: loading, error, value states |
| `money-amount.test.tsx` | 3 | MoneyAmount: formatting, negative values, currency symbols |
| `filters.test.tsx` | 6 | DateRangeFilter, SearchInput, RefreshButton |
| `phase-7-7.test.tsx` | 21 | Phase 7.7 components: PageHeader, DetailCard, SafePayloadViewer redaction, webhook URL validation, audit immutability, system health, secret security |

Total: 65 tests across 10 test files.

## Testing Patterns

### Component Rendering Tests

```tsx
import { render, screen } from '@testing-library/react';

it('renders title', () => {
  render(<MyComponent title="Hello" />);
  expect(screen.getByText('Hello')).toBeInTheDocument();
});
```

### Mocking Dependencies

```tsx
// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn(), back: vi.fn() }),
  useParams: () => ({}),
}));

// Mock API modules
vi.mock('@/api/outbox', () => ({
  listOutboxEvents: vi.fn(),
  getOutboxEvent: vi.fn(),
}));
```

### Testing Permission Guards

```tsx
import { PermissionGuard } from '@/components/shared/permission-guards';

// Mock usePermission to control permission state
vi.mock('@/hooks/use-permission', () => ({
  usePermission: () => ({ hasPermission: true, can: () => true }),
}));
```

### Testing SafePayloadViewer Redaction

```tsx
import { SafePayloadViewer } from '@/components/shared/safe-payload-viewer';

it('redacts sensitive fields', () => {
  const data = { secret: 'my-secret', safe: 'visible' };
  const { container } = render(<SafePayloadViewer data={data} />);
  fireEvent.click(screen.getByText('Payload')); // Expand viewer
  expect(container.textContent).not.toContain('my-secret');
  expect(container.textContent).toContain('visible');
});
```

## Mock Strategy

- **next/navigation**: `useRouter`, `useParams`, `usePathname` are mocked
- **next/link**: Rendered as `<a>` tags for testing
- **@tanstack/react-query**: `useQuery` and `useQueryClient` are mocked
- **sonner**: `toast` is mocked to avoid external dependencies
- **API modules**: Each API module is mocked per test file
- **lucide-react**: Icons are mocked as simple `<span>` elements
- **Permission hooks**: Controlled mock for testing different access levels

## Coverage Areas

- Auth store operations (set, clear, update, permissions)
- API client (headers, error parsing)
- Permission guards (exact, anyOf, denied)
- Shared components (badge, money, table, filters, metric card)
- Safe payload viewer (redaction, empty states)
- Page-level rendering (dashboard, wallets, outbox, audit, system health)
- Form validation (URL, event types)
- Security behavior (secret handling, read-only audit, permission-gated actions)

---

> This document is part of Phase 7.8 final polish documentation.
