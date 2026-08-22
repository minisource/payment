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
import { listWalletConsistencyIssues, resolveWalletIssue, markWalletIssueFalsePositive, type WalletConsistencyIssue } from '@/api/wallet-consistency';
import { CheckCircle, Flag } from 'lucide-react';

const PAGE_SIZE = 20;

export default function WalletConsistencyPage() {
  return (
    <RoutePermissionGuard permissions={['payment.security.reports.view_admin']}>
      <WalletConsistencyContent />
    </RoutePermissionGuard>
  );
}

function WalletConsistencyContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['wallet-consistency', { skip, search, status: statusFilter }],
    queryFn: () => listWalletConsistencyIssues({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'resolve') {
        await resolveWalletIssue(action.id, { reason: reason || 'Resolved by admin' });
      } else if (action.type === 'false-positive') {
        await markWalletIssueFalsePositive(action.id, { reason: reason || 'Marked as false positive' });
      }
      toast.success('Action completed');
      queryClient.invalidateQueries({ queryKey: ['wallet-consistency'] });
      setReason('');
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<WalletConsistencyIssue>[] = [
    { key: 'id', header: 'ID', render: (i) => <span className="font-mono text-xs">{i.id.slice(0, 12)}…</span> },
    { key: 'wallet', header: 'Wallet', render: (i) => <Link href={`/admin/wallets/${i.walletId}`} className="text-primary hover:underline font-mono text-xs">{i.walletId.slice(0, 10)}…</Link> },
    { key: 'type', header: 'Type', render: (i) => <span className="text-xs capitalize">{i.issueType.replace(/_/g, ' ')}</span> },
    { key: 'severity', header: 'Severity', render: (i) => <StatusBadge status={i.severity} /> },
    { key: 'status', header: 'Status', render: (i) => <StatusBadge status={i.status} /> },
    { key: 'expected', header: 'Expected', render: (i) => <MoneyAmount amount={i.expectedBalance} currency={i.currency} className="text-xs" /> },
    { key: 'actual', header: 'Actual', render: (i) => <MoneyAmount amount={i.actualBalance} currency={i.currency} className="text-xs" /> },
    { key: 'detected', header: 'Detected', render: (i) => <span className="text-xs">{new Date(i.detectedAt).toLocaleDateString()}</span> },
    { key: 'actions', header: '', render: (i) => i.status !== 'resolved' && i.status !== 'false_positive' ? (
      <div className="flex gap-1">
        <PermissionGuard permission="payment.wallet.consistency.manage_admin">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: i.id, type: 'resolve' }); }} className="text-xs text-green-600"><CheckCircle className="h-3 w-3" /></Button>
        </PermissionGuard>
        <PermissionGuard permission="payment.wallet.consistency.manage_admin">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); setAction({ id: i.id, type: 'false-positive' }); }} className="text-xs text-yellow-600"><Flag className="h-3 w-3" /></Button>
        </PermissionGuard>
      </div>
    ) : null },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Wallet Consistency</h1>
          <p className="text-sm text-muted-foreground">Balance mismatches and integrity issues</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option><option value="open">Open</option><option value="resolved">Resolved</option><option value="false_positive">False Positive</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(i) => i.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => { setAction(null); setReason(''); }} onConfirm={handleAction} confirmDisabled={!reason && action.type !== 'resolve'}
          title={`${action.type === 'resolve' ? 'Resolve' : 'Mark False Positive'} Issue`}
          description={<div className="mt-3"><label className="text-xs font-medium">Reason *</label><input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" /></div>}
          confirmLabel={action.type === 'resolve' ? 'Resolve' : 'Mark False Positive'} destructive={action.type === 'false-positive'} />
      )}
    </div>
  );
}
