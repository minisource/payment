'use client';

import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { Loading } from '@/components/shared/states';
import { RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { getSecurityOverview } from '@/api/security';
import { Shield, AlertTriangle, Clock, Ban, CheckCircle, XCircle, Scale, Activity } from 'lucide-react';

export default function SecurityPage() {
  return (
    <RoutePermissionGuard permissions={['payment.security.reports.view_admin']}>
      <SecurityContent />
    </RoutePermissionGuard>
  );
}

function SecurityContent() {
  const { data, isLoading } = useQuery({
    queryKey: ['security-overview'],
    queryFn: getSecurityOverview,
  });

  if (isLoading) return <Loading />;

  const metrics = [
    { label: 'Open Risk Cases', value: data?.openRiskCases ?? '—', icon: AlertTriangle, color: data?.openRiskCases ? (data.criticalRiskCases > 0 ? 'text-red-600' : 'text-yellow-600') : 'text-muted-foreground' },
    { label: 'Critical Cases', value: data?.criticalRiskCases ?? '—', icon: Shield, color: data?.criticalRiskCases ? 'text-red-600' : 'text-muted-foreground' },
    { label: 'Unresolved Rec Items', value: data?.unresolvedReconciliationItems ?? '—', icon: Scale, color: data?.unresolvedReconciliationItems ? 'text-yellow-600' : 'text-muted-foreground' },
    { label: 'Wallet Issues', value: data?.walletsWithConsistencyIssues ?? '—', icon: Activity, color: data?.walletsWithConsistencyIssues ? 'text-yellow-600' : 'text-muted-foreground' },
    { label: 'Failed Hash Checks', value: data?.failedLedgerHashChecks ?? '—', icon: XCircle, color: data?.failedLedgerHashChecks ? 'text-red-600' : 'text-muted-foreground' },
    { label: 'Blocked Ops Today', value: data?.blockedOperationsToday ?? '—', icon: Ban, color: data?.blockedOperationsToday ? 'text-red-600' : 'text-muted-foreground' },
    { label: 'Pending Approvals', value: data?.pendingAdminApprovals ?? '—', icon: Clock, color: data?.pendingAdminApprovals ? 'text-blue-600' : 'text-muted-foreground' },
    { label: 'Failed Security Ops', value: data?.failedSecurityOperations ?? '—', icon: AlertTriangle, color: data?.failedSecurityOperations ? 'text-red-600' : 'text-muted-foreground' },
  ];

  const sections = [
    { href: '/admin/security/ledger-integrity', label: 'Ledger Integrity', desc: 'Hash chain verification, backfill, deep checks' },
    { href: '/admin/security/wallet-consistency', label: 'Wallet Consistency', desc: 'Balance mismatches, expected vs actual' },
    { href: '/admin/security/limit-usage', label: 'Limit Usage', desc: 'Velocity tracking, limit violations' },
    { href: '/admin/security/reconciliation/batches', label: 'Reconciliation', desc: 'Batches and item-level resolution' },
    { href: '/admin/security/risk/evaluations', label: 'Risk Evaluations', desc: 'Risk scoring, triggered rules, decisions' },
    { href: '/admin/security/risk/cases', label: 'Risk Cases', desc: 'Open, acknowledged, escalated, resolved' },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Financial Security</h1>
        <p className="text-sm text-muted-foreground">Risk monitoring, reconciliation, ledger integrity, and admin approvals</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {metrics.map((m) => {
          const Icon = m.icon;
          return (
            <Card key={m.label}>
              <CardContent className="p-4 flex items-center gap-3">
                <Icon className={`h-5 w-5 ${m.color}`} />
                <div>
                  <p className="text-xs text-muted-foreground">{m.label}</p>
                  <p className="text-xl font-bold tabular-nums">{m.value}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((s) => (
          <Link key={s.href} href={s.href}>
            <Card className="h-full transition-colors hover:border-primary/50">
              <CardContent className="p-4">
                <h3 className="font-semibold">{s.label}</h3>
                <p className="mt-1 text-xs text-muted-foreground">{s.desc}</p>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      {/* Latest items tables */}
      {data?.latestCriticalCases && data.latestCriticalCases.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Latest Critical Risk Cases</CardTitle></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-xs">
              <thead><tr className="border-b text-muted-foreground"><th className="p-2 text-left">Case</th><th className="p-2 text-left">Severity</th><th className="p-2 text-left">Status</th><th className="p-2 text-left">Date</th></tr></thead>
              <tbody>{data.latestCriticalCases.map((c) => (
                <tr key={c.id} className="border-b hover:bg-accent/50">
                  <td className="p-2"><Link href={`/admin/security/risk/cases/${c.id}`} className="text-primary hover:underline">{c.title}</Link></td>
                  <td className="p-2"><RiskSeverityBadge severity={c.severity} /></td>
                  <td className="p-2 capitalize">{c.status.replace(/_/g, ' ')}</td>
                  <td className="p-2 text-muted-foreground">{new Date(c.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}</tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {data?.latestUnresolvedItems && data.latestUnresolvedItems.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Latest Unresolved Reconciliation Items</CardTitle></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-xs">
              <thead><tr className="border-b text-muted-foreground"><th className="p-2 text-left">Item</th><th className="p-2 text-left">Expected</th><th className="p-2 text-left">Actual</th><th className="p-2 text-left">Severity</th></tr></thead>
              <tbody>{data.latestUnresolvedItems.map((i) => (
                <tr key={i.id} className="border-b hover:bg-accent/50">
                  <td className="p-2"><Link href={`/admin/security/reconciliation/items/${i.id}`} className="text-primary hover:underline font-mono text-xs">{i.id.slice(0, 12)}…</Link></td>
                  <td className="p-2"><MoneyAmount amount={i.expectedAmount} currency={i.currency} /></td>
                  <td className="p-2"><MoneyAmount amount={i.actualAmount} currency={i.currency} /></td>
                  <td className="p-2"><RiskSeverityBadge severity={i.severity} /></td>
                </tr>
              ))}</tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {data?.latestPendingApprovals && data.latestPendingApprovals.length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Latest Pending Approvals</CardTitle></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-xs">
              <thead><tr className="border-b text-muted-foreground"><th className="p-2 text-left">Request</th><th className="p-2 text-left">Operation</th><th className="p-2 text-left">Date</th></tr></thead>
              <tbody>{data.latestPendingApprovals.map((a) => (
                <tr key={a.id} className="border-b hover:bg-accent/50">
                  <td className="p-2"><Link href={`/admin/approvals/${a.id}`} className="text-primary hover:underline font-mono text-xs">{a.id.slice(0, 12)}…</Link></td>
                  <td className="p-2 capitalize">{a.operationType.replace(/_/g, ' ')}</td>
                  <td className="p-2 text-muted-foreground">{new Date(a.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}</tbody>
            </table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
