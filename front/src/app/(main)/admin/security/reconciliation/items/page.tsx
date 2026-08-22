'use client';

import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { listReconciliationItems, resolveReconciliationItem, markReconciliationItemFalsePositive, ignoreReconciliationItem, type ReconciliationItem } from '@/api/reconciliation';
import { CheckCircle, Flag, EyeOff } from 'lucide-react';

const PAGE_SIZE = 20;

export default function ReconciliationItemsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.reconciliation.view']}>
      <ItemsContent />
    </RoutePermissionGuard>
  );
}

function ItemsContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['rec-items', { skip, search, status: statusFilter }],
    queryFn: () => listReconciliationItems({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action || !reason) return;
    try {
      if (action.type === 'resolve') await resolveReconciliationItem(action.id, { reason });
      else if (action.type === 'false-positive') await markReconciliationItemFalsePositive(action.id, { reason });
      else if (action.type === 'ignore') await ignoreReconciliationItem(action.id, { reason });
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['rec-items'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<ReconciliationItem>[] = [
    { key: 'id', header: 'ID', render: (i) => <Link href={`/admin/security/reconciliation/items/${i.id}`} className="font-mono text-xs text-primary hover:underline">{i.id.slice(0, 12)}…</Link> },
    { key: 'batch', header: 'Batch', render: (i) => <Link href={`/admin/security/reconciliation/batches/${i.batchId}`} className="font-mono text-xs text-primary hover:underline">{i.batchId.slice(0, 10)}…</Link> },
    { key: 'type', header: 'Type', render: (i) => <span className="text-xs capitalize">{i.itemType.replace(/_/g, ' ')}</span> },
    { key: 'status', header: 'Status', render: (i) => <StatusBadge status={i.status} /> },
    { key: 'severity', header: 'Severity', render: (i) => <StatusBadge status={i.severity} /> },
    { key: 'expected', header: 'Expected', render: (i) => <MoneyAmount amount={i.expectedAmount} currency={i.currency} className="text-xs" /> },
    { key: 'actual', header: 'Actual', render: (i) => <MoneyAmount amount={i.actualAmount} currency={i.currency} className="text-xs" /> },
    { key: 'diff', header: 'Diff', render: (i) => <MoneyAmount amount={i.differenceAmount} currency={i.currency} className={`text-xs font-medium ${i.differenceAmount !== 0 ? 'text-red-600' : ''}`} /> },
    { key: 'actions', header: '', render: (i) => i.status !== 'resolved' && i.status !== 'false_positive' && i.status !== 'ignored' ? (
      <div className="flex gap-1">
        <PermissionGuard permission="payment.reconciliation.resolve">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: i.id, type: 'resolve' }); }} className="text-xs text-green-600"><CheckCircle className="h-3 w-3" /></Button>
        </PermissionGuard>
        <PermissionGuard permission="payment.reconciliation.false_positive">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: i.id, type: 'false-positive' }); }} className="text-xs text-yellow-600"><Flag className="h-3 w-3" /></Button>
        </PermissionGuard>
        <PermissionGuard permission="payment.reconciliation.resolve">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: i.id, type: 'ignore' }); }} className="text-xs text-gray-600"><EyeOff className="h-3 w-3" /></Button>
        </PermissionGuard>
      </div>
    ) : null },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Reconciliation Items</h1>
          <p className="text-sm text-muted-foreground">Individual reconciliation discrepancies</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="mismatched">Mismatched</option><option value="matched">Matched</option><option value="unresolved">Unresolved</option><option value="resolved">Resolved</option><option value="false_positive">False Positive</option><option value="ignored">Ignored</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(i) => i.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction} confirmDisabled={!reason}
          title={`${action.type === 'resolve' ? 'Resolve' : action.type === 'false-positive' ? 'Mark False Positive' : 'Ignore'} Item`}
          description={<div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>}
          confirmLabel={action.type === 'resolve' ? 'Resolve' : action.type === 'false-positive' ? 'False Positive' : 'Ignore'}
          destructive={action.type !== 'resolve'} />
      )}
    </div>
  );
}
