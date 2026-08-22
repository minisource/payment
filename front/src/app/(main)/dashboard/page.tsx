'use client';

import { useState, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { MetricCard } from '@/components/shared/metric-card';
import { ChartCard } from '@/components/shared/chart-card';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { DataTable, DataTableColumn } from '@/components/shared/data-table';
import { DateRangeFilter, RefreshButton } from '@/components/shared/filters';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { useTenant } from '@/hooks/use-tenant';
import {
  getDashboardOverview, getLatestPaymentIntents, getLatestWithdrawals,
  getLatestRiskCases, getLatestReconciliationItems, getLatestEventDeliveries,
  getPaymentVolumeChart, getWithdrawalVolumeChart, getGatewayChart,
  getRiskChart, getEventDeliveryChart,
  type DashboardOverviewResponse, type LatestPaymentIntentItem,
  type LatestWithdrawalItem, type LatestRiskCaseItem,
  type LatestReconciliationItem, type LatestEventDeliveryItem,
} from '@/api/dashboard';
import {
  Wallet, ArrowLeftRight, Shield, Webhook, Clock,
  Server, Landmark, Activity, FileText,
} from 'lucide-react';
import Link from 'next/link';
import {
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  PieChart, Pie, Cell, Legend, LineChart, Line,
} from 'recharts';

const CHART_COLORS = ['#3b82f6', '#22c55e', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'];

// ─── Section Wrapper ────────────────────────────────────────────

function DashboardSection({ title, children, className }: { title: string; children: React.ReactNode; className?: string }) {
  return (
    <Card className={className}>
      <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">{title}</CardTitle></CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}

// ─── Main Dashboard Page ────────────────────────────────────────

export default function DashboardPage() {
  const { tenantId, tenantName } = useTenant();
  const [dateFrom, setDateFrom] = useState<string | undefined>();
  const [dateTo, setDateTo] = useState<string | undefined>();
  const [lastRefreshed, setLastRefreshed] = useState<Date | undefined>();

  const params = { dateFrom, dateTo };

  // Dashboard overview
  const overview = useQuery<DashboardOverviewResponse>({
    queryKey: ['dashboard-overview', params],
    queryFn: () => getDashboardOverview(params),
    staleTime: 30_000,
    retry: false,
  });

  // Charts
  const paymentVolChart = useQuery({ queryKey: ['chart-payment-volume', params], queryFn: () => getPaymentVolumeChart(params), staleTime: 60_000, retry: false });
  const withdrawalVolChart = useQuery({ queryKey: ['chart-withdrawal-volume', params], queryFn: () => getWithdrawalVolumeChart(params), staleTime: 60_000, retry: false });
  const gatewayChart = useQuery({ queryKey: ['chart-gateway', params], queryFn: () => getGatewayChart(params), staleTime: 60_000, retry: false });
  const riskChart = useQuery({ queryKey: ['chart-risk', params], queryFn: () => getRiskChart(params), staleTime: 60_000, retry: false });
  const eventChart = useQuery({ queryKey: ['chart-event', params], queryFn: () => getEventDeliveryChart(params), staleTime: 60_000, retry: false });

  // Latest tables
  const latestPayments = useQuery({ queryKey: ['latest-payments', params], queryFn: () => getLatestPaymentIntents({ ...params, limit: 5 }), staleTime: 30_000, retry: false });
  const latestWithdrawals = useQuery({ queryKey: ['latest-withdrawals', params], queryFn: () => getLatestWithdrawals({ ...params, limit: 5 }), staleTime: 30_000, retry: false });
  const latestRiskCases = useQuery({ queryKey: ['latest-risk', params], queryFn: () => getLatestRiskCases({ ...params, limit: 5 }), staleTime: 30_000, retry: false });
  const latestRecItems = useQuery({ queryKey: ['latest-rec', params], queryFn: () => getLatestReconciliationItems({ ...params, limit: 5 }), staleTime: 30_000, retry: false });
  const latestDeliveries = useQuery({ queryKey: ['latest-deliveries', params], queryFn: () => getLatestEventDeliveries({ ...params, limit: 5 }), staleTime: 30_000, retry: false });

  const handleRefresh = useCallback(() => {
    overview.refetch();
    paymentVolChart.refetch();
    withdrawalVolChart.refetch();
    gatewayChart.refetch();
    riskChart.refetch();
    eventChart.refetch();
    latestPayments.refetch();
    latestWithdrawals.refetch();
    latestRiskCases.refetch();
    latestRecItems.refetch();
    latestDeliveries.refetch();
    setLastRefreshed(new Date());
  }, [overview.refetch, paymentVolChart.refetch, withdrawalVolChart.refetch, gatewayChart.refetch, riskChart.refetch, eventChart.refetch, latestPayments.refetch, latestWithdrawals.refetch, latestRiskCases.refetch, latestRecItems.refetch, latestDeliveries.refetch]);

  const data = overview.data;

  // ─── Payment intent columns ───────────────────────────────────
  const paymentColumns: DataTableColumn<LatestPaymentIntentItem>[] = [
    { key: 'id', header: 'ID', render: (p) => <span className="font-mono text-xs">{p.id.slice(0, 12)}…</span> },
    { key: 'amount', header: 'Amount', render: (p) => <MoneyAmount amount={p.amount} currency={p.currency} className="text-xs" /> },
    { key: 'status', header: 'Status', render: (p) => <StatusBadge status={p.status} /> },
    { key: 'provider', header: 'Provider', render: (p) => <span className="text-xs">{p.providerCode || '—'}</span> },
  ];

  const withdrawalColumns: DataTableColumn<LatestWithdrawalItem>[] = [
    { key: 'id', header: 'ID', render: (p) => <span className="font-mono text-xs">{p.id.slice(0, 12)}…</span> },
    { key: 'amount', header: 'Amount', render: (p) => <MoneyAmount amount={p.amount} currency={p.currency} className="text-xs" /> },
    { key: 'status', header: 'Status', render: (p) => <StatusBadge status={p.status} /> },
  ];

  const riskColumns: DataTableColumn<LatestRiskCaseItem>[] = [
    { key: 'id', header: 'ID', render: (p) => <span className="font-mono text-xs">{p.id.slice(0, 12)}…</span> },
    { key: 'severity', header: 'Severity', render: (p) => <RiskSeverityBadge severity={p.severity} /> },
    { key: 'status', header: 'Status', render: (p) => <StatusBadge status={p.status} /> },
    { key: 'description', header: 'Description', render: (p) => <span className="max-w-[200px] truncate text-xs">{p.description}</span> },
  ];

  const recColumns: DataTableColumn<LatestReconciliationItem>[] = [
    { key: 'id', header: 'ID', render: (p) => <span className="font-mono text-xs">{p.id.slice(0, 12)}…</span> },
    { key: 'type', header: 'Type', render: (p) => <span className="text-xs">{p.reconciliationType}</span> },
    { key: 'status', header: 'Status', render: (p) => <StatusBadge status={p.status} /> },
  ];

  const deliveryColumns: DataTableColumn<LatestEventDeliveryItem>[] = [
    { key: 'id', header: 'ID', render: (p) => <span className="font-mono text-xs">{p.id.slice(0, 12)}…</span> },
    { key: 'eventType', header: 'Event', render: (p) => <span className="text-xs">{p.eventType}</span> },
    { key: 'status', header: 'Status', render: (p) => <StatusBadge status={p.status} /> },
  ];

  return (
    <div className="space-y-6">
      {/* ── Header ───────────────────────────────────────────── */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Payment Admin Dashboard</h1>
          <p className="text-sm text-muted-foreground">
            {tenantName ? `Tenant: ${tenantName}` : 'All Tenants'}
            {tenantId && <span className="ml-2 font-mono text-xs">({tenantId})</span>}
            <span className="ml-2 rounded-full bg-muted px-2 py-0.5 font-mono text-xs">payment-admin</span>
          </p>
        </div>
        <div className="flex items-center gap-3">
          <DateRangeFilter dateFrom={dateFrom} dateTo={dateTo} onChange={(f, t) => { setDateFrom(f); setDateTo(t); }} />
          <RefreshButton onClick={handleRefresh} isLoading={overview.isFetching} lastRefreshed={lastRefreshed} />
        </div>
      </div>

      {/* ── Financial Metrics ────────────────────────────────── */}
      <PermissionGuard permission={['payment.security.reports.view_admin', 'payment.wallet.view_admin']} mode="anyOf">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {data?.liability?.map((li) => (
            <MetricCard key={li.currency}
              title={`Wallet Liability (${li.currency})`}
              value={<MoneyAmount amount={li.availableTotal} currency={li.currency} />}
              icon={Wallet}
              subtext={`${(li.lockedTotal / Math.max(li.availableTotal + li.lockedTotal, 1) * 100).toFixed(1)}% locked`}
              isLoading={overview.isLoading} isError={overview.isError}
            />
          )) || (
            <MetricCard title="Wallet Liability" icon={Wallet} isLoading={overview.isLoading} isError={overview.isError} />
          )}
        </div>
      </PermissionGuard>

      {/* ── Payment + Withdrawal Metrics ─────────────────────── */}
      <div className="grid gap-4 md:grid-cols-2">
        <PermissionGuard permission="payment.intent.view_admin">
          <div className="grid grid-cols-2 gap-4">
            <MetricCard title="Successful Today" value={data?.payments?.[0]?.successfulToday} icon={ArrowLeftRight} isLoading={overview.isLoading} />
            <MetricCard title="Failed Today" value={data?.payments?.[0]?.failedToday} icon={ArrowLeftRight} isLoading={overview.isLoading} />
            <MetricCard title="Success Rate" value={data?.payments?.[0]?.successRate != null ? `${(data.payments[0].successRate * 100).toFixed(1)}%` : undefined} isLoading={overview.isLoading} />
            <MetricCard title="Volume" value={data?.payments?.[0] ? <MoneyAmount amount={data.payments[0].totalVolume} currency={data.payments[0].currency} /> : undefined} isLoading={overview.isLoading} />
          </div>
        </PermissionGuard>

        <PermissionGuard permission="payment.withdrawal.view_admin">
          <div className="grid grid-cols-2 gap-4">
            <MetricCard title="Pending" value={data?.withdrawals?.[0]?.pendingCount} icon={Clock} isLoading={overview.isLoading} />
            <MetricCard title="Pending Amount" value={data?.withdrawals?.[0] ? <MoneyAmount amount={data.withdrawals[0].pendingAmount} currency={data.withdrawals[0].currency} /> : undefined} icon={Landmark} isLoading={overview.isLoading} />
            <MetricCard title="Approved" value={data?.withdrawals?.[0]?.approvedCount} icon={Clock} isLoading={overview.isLoading} />
            <MetricCard title="Paid" value={data?.withdrawals?.[0] ? <MoneyAmount amount={data.withdrawals[0].paidAmount} currency={data.withdrawals[0].currency} /> : undefined} icon={Landmark} isLoading={overview.isLoading} />
          </div>
        </PermissionGuard>
      </div>

      {/* ── Gateway + Security + Events ───────────────────────── */}
      <div className="grid gap-4 md:grid-cols-3">
        <PermissionGuard permission="payment.gateway.config.view">
          <div className="space-y-4">
            <MetricCard title="Gateway Success Rate" value={data?.gateways?.successRate != null ? `${(data.gateways.successRate * 100).toFixed(1)}%` : undefined} icon={Server} isLoading={overview.isLoading} />
            <MetricCard title="Failed Transactions" value={data?.gateways?.failedTransactions} icon={Server} isLoading={overview.isLoading} />
            <MetricCard title="Top Provider" value={data?.gateways?.topProvider || 'N/A'} icon={Server} isLoading={overview.isLoading} />
          </div>
        </PermissionGuard>

        <PermissionGuard permission={['payment.risk.view', 'payment.reconciliation.view', 'payment.admin_approval.view']} mode="anyOf">
          <DashboardSection title="Security & Risk">
            <div className="space-y-3">
              <PermissionGuard permission="payment.risk.view">
                <div className="flex items-center justify-between">
                  <span className="text-xs">Risk Cases</span>
                  <div className="flex gap-2">
                    <span className="text-xs font-medium">{data?.security?.openRiskCases ?? '—'} open</span>
                    <span className="text-xs font-medium text-red-600">{data?.security?.criticalRiskCases ?? '—'} critical</span>
                  </div>
                </div>
              </PermissionGuard>
              <PermissionGuard permission="payment.reconciliation.view">
                <div className="flex items-center justify-between">
                  <span className="text-xs">Reconciliation</span>
                  <span className="text-xs font-medium">{data?.security?.unresolvedRecItems ?? '—'} unresolved</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-xs">Wallet Issues</span>
                  <span className="text-xs font-medium">{data?.security?.walletConsistencyIssues ?? '—'}</span>
                </div>
              </PermissionGuard>
              <PermissionGuard permission="payment.admin_approval.view">
                <div className="flex items-center justify-between">
                  <span className="text-xs">Pending Approvals</span>
                  <span className="text-xs font-medium">{data?.security?.pendingApprovals ?? '—'}</span>
                </div>
              </PermissionGuard>
            </div>
          </DashboardSection>
        </PermissionGuard>

        <PermissionGuard permission="payment.outbox.view_admin">
          <DashboardSection title="Event Delivery">
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-xs">Pending Outbox</span>
                <span className="text-xs font-medium">{data?.events?.pendingOutbox ?? '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs">Failed Outbox</span>
                <span className="text-xs font-medium text-red-600">{data?.events?.failedOutbox ?? '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs">Dead-lettered</span>
                <span className="text-xs font-medium text-red-600">{data?.events?.deadLetteredOutbox ?? '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs">Failed Webhooks</span>
                <span className="text-xs font-medium text-red-600">{data?.events?.failedWebhookDeliveries ?? '—'}</span>
              </div>
            </div>
          </DashboardSection>
        </PermissionGuard>
      </div>

      {/* ── Charts ────────────────────────────────────────────── */}
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <PermissionGuard permission="payment.intent.view_admin">
          <ChartCard title="Payment Volume" isLoading={paymentVolChart.isLoading} isError={paymentVolChart.isError} isEmpty={!paymentVolChart.data?.length} onRetry={() => paymentVolChart.refetch()}>
            {paymentVolChart.data && (
              <ResponsiveContainer>
                <BarChart data={paymentVolChart.data}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="label" fontSize={11} />
                  <YAxis fontSize={11} />
                  <Tooltip />
                  <Bar dataKey="value" fill={CHART_COLORS[0]} radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </PermissionGuard>

        <PermissionGuard permission="payment.withdrawal.view_admin">
          <ChartCard title="Withdrawal Volume" isLoading={withdrawalVolChart.isLoading} isError={withdrawalVolChart.isError} isEmpty={!withdrawalVolChart.data?.length} onRetry={() => withdrawalVolChart.refetch()}>
            {withdrawalVolChart.data && (
              <ResponsiveContainer>
                <LineChart data={withdrawalVolChart.data}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="label" fontSize={11} />
                  <YAxis fontSize={11} />
                  <Tooltip />
                  <Line type="monotone" dataKey="value" stroke={CHART_COLORS[1]} strokeWidth={2} />
                </LineChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </PermissionGuard>

        <PermissionGuard permission="payment.gateway.config.view">
          <ChartCard title="Gateway Status" isLoading={gatewayChart.isLoading} isError={gatewayChart.isError} isEmpty={!gatewayChart.data?.length} onRetry={() => gatewayChart.refetch()}>
            {gatewayChart.data && (
              <ResponsiveContainer>
                <PieChart>
                  <Pie data={gatewayChart.data} dataKey="value" nameKey="label" cx="50%" cy="50%" outerRadius={70} label>
                    {gatewayChart.data.map((_, i) => <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />)}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </PermissionGuard>

        <PermissionGuard permission="payment.risk.view">
          <ChartCard title="Risk by Severity" isLoading={riskChart.isLoading} isError={riskChart.isError} isEmpty={!riskChart.data?.length} onRetry={() => riskChart.refetch()}>
            {riskChart.data && (
              <ResponsiveContainer>
                <PieChart>
                  <Pie data={riskChart.data} dataKey="value" nameKey="label" cx="50%" cy="50%" outerRadius={70}>
                    {riskChart.data.map((_, i) => <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />)}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </PermissionGuard>

        <PermissionGuard permission="payment.outbox.view_admin">
          <ChartCard title="Event Delivery Status" isLoading={eventChart.isLoading} isError={eventChart.isError} isEmpty={!eventChart.data?.length} onRetry={() => eventChart.refetch()}>
            {eventChart.data && (
              <ResponsiveContainer>
                <BarChart data={eventChart.data}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="label" fontSize={11} />
                  <YAxis fontSize={11} />
                  <Tooltip />
                  <Bar dataKey="value" fill={CHART_COLORS[4]} radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </PermissionGuard>
      </div>

      {/* ── Latest Activity Tables ────────────────────────────── */}
      <div className="grid gap-6 lg:grid-cols-2">
        <PermissionGuard permission="payment.intent.view_admin">
          <DashboardSection title="Latest Payment Intents">
            <DataTable
              columns={paymentColumns}
              data={latestPayments.data?.items || []}
              keyExtractor={(p) => p.id}
              isLoading={latestPayments.isLoading}
              isEmpty={!latestPayments.data?.items?.length}
              isError={latestPayments.isError}
              onRetry={() => latestPayments.refetch()}
              compact
            />
            <Link href="/admin/payments" className="mt-2 inline-block text-xs text-primary hover:underline">View all →</Link>
          </DashboardSection>
        </PermissionGuard>

        <PermissionGuard permission="payment.withdrawal.view_admin">
          <DashboardSection title="Latest Withdrawals">
            <DataTable
              columns={withdrawalColumns}
              data={latestWithdrawals.data?.items || []}
              keyExtractor={(w) => w.id}
              isLoading={latestWithdrawals.isLoading}
              isEmpty={!latestWithdrawals.data?.items?.length}
              isError={latestWithdrawals.isError}
              onRetry={() => latestWithdrawals.refetch()}
              compact
            />
            <Link href="/admin/withdrawals" className="mt-2 inline-block text-xs text-primary hover:underline">View all →</Link>
          </DashboardSection>
        </PermissionGuard>

        <PermissionGuard permission="payment.risk.view">
          <DashboardSection title="Latest Risk Cases">
            <DataTable
              columns={riskColumns}
              data={latestRiskCases.data?.items || []}
              keyExtractor={(r) => r.id}
              isLoading={latestRiskCases.isLoading}
              isEmpty={!latestRiskCases.data?.items?.length}
              isError={latestRiskCases.isError}
              onRetry={() => latestRiskCases.refetch()}
              compact
            />
            <Link href="/admin/security" className="mt-2 inline-block text-xs text-primary hover:underline">View all →</Link>
          </DashboardSection>
        </PermissionGuard>

        <PermissionGuard permission="payment.reconciliation.view">
          <DashboardSection title="Latest Reconciliation Items">
            <DataTable
              columns={recColumns}
              data={latestRecItems.data?.items || []}
              keyExtractor={(r) => r.id}
              isLoading={latestRecItems.isLoading}
              isEmpty={!latestRecItems.data?.items?.length}
              isError={latestRecItems.isError}
              onRetry={() => latestRecItems.refetch()}
              compact
            />
            <Link href="/admin/security/reconciliation" className="mt-2 inline-block text-xs text-primary hover:underline">View all →</Link>
          </DashboardSection>
        </PermissionGuard>

        <PermissionGuard permission="payment.outbox.view_admin">
          <DashboardSection title="Latest Failed Deliveries">
            <DataTable
              columns={deliveryColumns}
              data={latestDeliveries.data?.items || []}
              keyExtractor={(d) => d.id}
              isLoading={latestDeliveries.isLoading}
              isEmpty={!latestDeliveries.data?.items?.length}
              isError={latestDeliveries.isError}
              onRetry={() => latestDeliveries.refetch()}
              compact
            />
            <Link href="/admin/events/webhooks" className="mt-2 inline-block text-xs text-primary hover:underline">View all →</Link>
          </DashboardSection>
        </PermissionGuard>
      </div>

      {/* ── Quick Navigation ──────────────────────────────────── */}
      <DashboardSection title="Quick Navigation">
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-8">
          {[
            { label: 'Wallets', href: '/admin/wallets', icon: Wallet, permission: 'payment.wallet.view_admin', desc: 'Manage wallets & balances' },
            { label: 'Ledger', href: '/admin/ledger', icon: Activity, permission: 'payment.wallet.view_admin', desc: 'Explore ledger entries' },
            { label: 'Payments', href: '/admin/payments', icon: ArrowLeftRight, permission: 'payment.intent.view_admin', desc: 'View payment intents' },
            { label: 'Gateways', href: '/admin/gateways', icon: Server, permission: 'payment.gateway.config.view', desc: 'Manage gateway configs' },
            { label: 'Withdrawals', href: '/admin/withdrawals', icon: Landmark, permission: 'payment.withdrawal.view_admin', desc: 'Withdrawal queue' },
            { label: 'Security', href: '/admin/security', icon: Shield, permission: 'payment.security.reports.view_admin', desc: 'Risk & reconciliation' },
            { label: 'Events', href: '/admin/events/outbox', icon: Webhook, permission: 'payment.outbox.view_admin', desc: 'Outbox & webhooks' },
            { label: 'Reports', href: '/admin/reports', icon: FileText, permission: 'payment.security.reports.view_admin', desc: 'Financial reports' },
          ].map((item) => (
            <PermissionGuard key={item.href} permission={item.permission}>
              <Link href={item.href}
                className="flex flex-col items-center gap-1 rounded-lg border p-3 text-center transition-colors hover:bg-accent hover:shadow-sm">
                <item.icon className="h-5 w-5 text-primary" />
                <span className="text-xs font-medium">{item.label}</span>
                <span className="text-[10px] text-muted-foreground">{item.desc}</span>
              </Link>
            </PermissionGuard>
          ))}
        </div>
      </DashboardSection>
    </div>
  );
}
