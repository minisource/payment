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
import { listLimitUsage, resetLimitUsage, increaseTemporaryLimit, type LimitUsageItem } from '@/api/limit-usage';
import { RotateCcw, TrendingUp } from 'lucide-react';

const PAGE_SIZE = 20;

export default function LimitUsagePage() {
  return (
    <RoutePermissionGuard permissions={['payment.security.reports.view_admin']}>
      <LimitUsageContent />
    </RoutePermissionGuard>
  );
}

function LimitUsageContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['limit-usage', { skip, search, status: statusFilter }],
    queryFn: () => listLimitUsage({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'reset') {
        await resetLimitUsage(action.id, { reason: reason || 'Admin reset' });
      } else if (action.type === 'increase') {
        await increaseTemporaryLimit(action.id, { reason: reason || 'Admin override' });
      }
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['limit-usage'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<LimitUsageItem>[] = [
    { key: 'subject', header: 'Subject', render: (u) => <span className="text-xs">{u.subjectType}: {u.subjectId?.slice(0, 8)}</span> },
    { key: 'operation', header: 'Operation', render: (u) => <span className="text-xs capitalize">{u.operationType.replace(/_/g, ' ')}</span> },
    { key: 'window', header: 'Window', render: (u) => <span className="text-xs capitalize">{u.windowType}</span> },
    { key: 'used', header: 'Used', render: (u) => <MoneyAmount amount={u.usedAmount} currency={u.currency} className="text-xs" /> },
    { key: 'limit', header: 'Limit', render: (u) => <MoneyAmount amount={u.limitAmount} currency={u.currency} className="text-xs" /> },
    { key: 'remaining', header: 'Remaining', render: (u) => <MoneyAmount amount={u.remainingAmount} currency={u.currency} className={`text-xs font-medium ${u.remainingAmount <= 0 ? 'text-red-600' : ''}`} /> },
    { key: 'status', header: 'Status', render: (u) => <StatusBadge status={u.status} /> },
    { key: 'reset', header: 'Reset', render: (u) => <span className="text-xs">{u.resetAt ? new Date(u.resetAt).toLocaleDateString() : '—'}</span> },
    { key: 'actions', header: '', render: (u) => (
      <div className="flex gap-1">
        <PermissionGuard permission="payment.limit_usage.manage_admin">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: u.id, type: 'reset' }); }} className="text-xs" title="Reset"><RotateCcw className="h-3 w-3" /></Button>
        </PermissionGuard>
        <PermissionGuard permission="payment.limit_usage.manage_admin">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: u.id, type: 'increase' }); }} className="text-xs" title="Increase Limit"><TrendingUp className="h-3 w-3" /></Button>
        </PermissionGuard>
      </div>
    ) },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Limit Usage</h1>
          <p className="text-sm text-muted-foreground">Velocity tracking and limit violations</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="normal">Normal</option><option value="near_limit">Near Limit</option><option value="exceeded">Exceeded</option><option value="blocked">Blocked</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(u) => u.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction} confirmDisabled={!reason}
          title={action.type === 'reset' ? 'Reset Limit Usage' : 'Increase Temporary Limit'}
          description={<div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>}
          confirmLabel={action.type === 'reset' ? 'Reset' : 'Increase'} destructive />
      )}
    </div>
  );
}
