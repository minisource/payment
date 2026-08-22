'use client';

import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge, RiskSeverityBadge } from '@/components/shared/enhanced-badge';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { listRiskCases, acknowledgeRiskCase, escalateRiskCase, resolveRiskCase, markRiskCaseFalsePositive, closeRiskCase, type RiskCase } from '@/api/risk';
import { CheckCircle, Flag, TrendingUp, X } from 'lucide-react';

const PAGE_SIZE = 20;

export default function RiskCasesPage() {
  return (
    <RoutePermissionGuard permissions={['payment.risk.view']}>
      <CasesContent />
    </RoutePermissionGuard>
  );
}

function CasesContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['risk-cases', { skip, search, status: statusFilter, severity: severityFilter }],
    queryFn: () => listRiskCases({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined, severity: severityFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'acknowledge') await acknowledgeRiskCase(action.id);
      else if (action.type === 'escalate') await escalateRiskCase(action.id);
      else if (action.type === 'resolve') await resolveRiskCase(action.id, { reason: reason || 'Resolved' });
      else if (action.type === 'false-positive') await markRiskCaseFalsePositive(action.id, { reason: reason || 'False positive' });
      else if (action.type === 'close') await closeRiskCase(action.id, { reason: reason || 'Closed' });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['risk-cases'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<RiskCase>[] = [
    { key: 'id', header: 'ID', render: (c) => <Link href={`/admin/security/risk/cases/${c.id}`} className="font-mono text-xs text-primary hover:underline">{c.id.slice(0, 12)}…</Link> },
    { key: 'title', header: 'Title', render: (c) => <span className="text-xs font-medium">{c.title}</span> },
    { key: 'type', header: 'Type', render: (c) => <span className="text-xs capitalize">{c.caseType.replace(/_/g, ' ')}</span> },
    { key: 'severity', header: 'Severity', render: (c) => <RiskSeverityBadge severity={c.severity} /> },
    { key: 'status', header: 'Status', render: (c) => <StatusBadge status={c.status} /> },
    { key: 'assigned', header: 'Assigned', render: (c) => <span className="text-xs">{c.assignedToUserId ? c.assignedToUserId.slice(0, 8) : '—'}</span> },
    { key: 'created', header: 'Date', render: (c) => <span className="text-xs">{new Date(c.createdAt).toLocaleDateString()}</span> },
    { key: 'actions', header: '', render: (c) => c.status !== 'resolved' && c.status !== 'false_positive' && c.status !== 'closed' ? (
      <div className="flex gap-1">
        {c.status === 'open' && (
          <PermissionGuard permission="payment.risk.manage"><Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: c.id, type: 'acknowledge' }); }} className="text-xs" title="Acknowledge"><CheckCircle className="h-3 w-3" /></Button></PermissionGuard>
        )}
        <PermissionGuard permission="payment.risk.escalate"><Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: c.id, type: 'escalate' }); }} className="text-xs text-orange-600" title="Escalate"><TrendingUp className="h-3 w-3" /></Button></PermissionGuard>
        <PermissionGuard permission="payment.risk.resolve"><Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: c.id, type: 'resolve' }); }} className="text-xs text-green-600" title="Resolve"><CheckCircle className="h-3 w-3" /></Button></PermissionGuard>
        <PermissionGuard permission="payment.risk.false_positive"><Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: c.id, type: 'false-positive' }); }} className="text-xs text-yellow-600" title="False Positive"><Flag className="h-3 w-3" /></Button></PermissionGuard>
        <PermissionGuard permission="payment.risk.resolve"><Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: c.id, type: 'close' }); }} className="text-xs text-red-600" title="Close"><X className="h-3 w-3" /></Button></PermissionGuard>
      </div>
    ) : null },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Risk Cases</h1>
          <p className="text-sm text-muted-foreground">Manual risk investigation and resolution</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={severityFilter} onChange={(e) => { setSeverityFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Severity</option><option value="low">Low</option><option value="medium">Medium</option><option value="high">High</option><option value="critical">Critical</option>
        </select>
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option><option value="open">Open</option><option value="acknowledged">Acknowledged</option><option value="investigating">Investigating</option><option value="resolved">Resolved</option><option value="false_positive">False Positive</option><option value="closed">Closed</option><option value="escalated">Escalated</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(c) => c.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction}
          confirmDisabled={['resolve', 'false-positive', 'close'].includes(action.type) && !reason}
          title={`${action.type === 'acknowledge' ? 'Acknowledge' : action.type === 'escalate' ? 'Escalate' : action.type === 'resolve' ? 'Resolve' : action.type === 'false-positive' ? 'Mark False Positive' : 'Close'} Case`}
          description={['resolve', 'false-positive', 'close'].includes(action.type) ? <div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div> : (action.type === 'escalate' ? 'Escalate this case for senior review?' : undefined)}
          confirmLabel={action.type === 'acknowledge' ? 'Acknowledge' : action.type === 'escalate' ? 'Escalate' : action.type === 'resolve' ? 'Resolve' : action.type === 'false-positive' ? 'False Positive' : 'Close'}
          destructive={['close', 'escalate'].includes(action.type)} />
      )}
    </div>
  );
}
