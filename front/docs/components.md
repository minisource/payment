# Shared Components — Phase 7.2

Reusable admin components added or enhanced in Phase 7.2.

## Data Display

| Component | Path | Description |
|-----------|------|-------------|
| `MetricCard` | `components/shared/metric-card.tsx` | Metric widget with loading/error/value states, trend indicator, icon |
| `ChartCard` | `components/shared/chart-card.tsx` | Chart wrapper with loading/empty/error states + retry |
| `ReportShell` | `components/shared/report-shell.tsx` | Reusable report page layout with FilterBar + placeholder |
| `StatusBadge` | `components/shared/enhanced-badge.tsx` | 40+ status mappings for payment intent, gateway, wallet, withdrawal, risk, reconciliation, outbox, webhook, approval |
| `RiskSeverityBadge` | `components/shared/enhanced-badge.tsx` | Severity badge (low/medium/high/critical) |
| `MoneyAmount` | `components/shared/money-amount.tsx` | String-safe amount display with negative values, skeleton loading, currency symbols, copyable |

## Tables

| Component | Path | Description |
|-----------|------|-------------|
| `DataTable` | `components/shared/data-table.tsx` | Generic table with loading/empty/error/success states, compact mode, sortable headers |
| `Pagination` | `components/shared/data-table.tsx` | Offset-based pagination controls |

## Filters

| Component | Path | Description |
|-----------|------|-------------|
| `FilterBar` | `components/shared/filters.tsx` | Wrapper for filter controls |
| `SearchInput` | `components/shared/filters.tsx` | Search input with clear button |
| `DateRangeFilter` | `components/shared/filters.tsx` | From/To date picker with clear |
| `RefreshButton` | `components/shared/filters.tsx` | Refresh with loading spinner + lastRefreshed label |
| `ExportButton` | `components/shared/filters.tsx` | Export button with disabled/disabledReason support |

## States

| Component | Path | Description |
|-----------|------|-------------|
| `Loading` | `components/shared/states.tsx` | Centered spinner |
| `EmptyState` | `components/shared/states.tsx` | Centered empty message |
| `ErrorState` | `components/shared/states.tsx` | Centered error with retry button |
| `Skeleton` | `components/shared/states.tsx` | Pulse animation placeholder |

## Permission Guards

| Component | Path | Description |
|-----------|------|-------------|
| `PermissionGuard` | `components/shared/permission-guards.tsx` | Hides children if user lacks permission |
| `RoutePermissionGuard` | `components/shared/permission-guards.tsx` | Route-level guard with login/auth-required messages |
| `ActionPermissionGuard` | `components/shared/permission-guards.tsx` | Action-level alias |

## Usage Patterns

```tsx
// MetricCard
<MetricCard title="Payments Today" value={456} icon={ArrowLeftRight} isLoading={loading} isError={!!error} />

// ChartCard with recharts
<ChartCard title="Payment Volume" isLoading={isLoading} isEmpty={!data?.length}>
  <ResponsiveContainer>
    <BarChart data={data}>...</BarChart>
  </ResponsiveContainer>
</ChartCard>

// StatusBadge
<StatusBadge status="pending_review" /> {/* Shows: pending review (yellow badge) */}

// MoneyAmount
<MoneyAmount amount="1250000" currency="IRR" tone="danger" /> {/* Shows: 1,250,000 ﷼ in red */}

// DataTable
<DataTable columns={cols} data={items} keyExtractor={i => i.id} isLoading={loading} compact />

// Permission-guarded section
<PermissionGuard permission="payment.intent.view_admin">
  <MetricCard ... />
</PermissionGuard>
```
