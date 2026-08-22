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
import { listReconciliationBatches, runReconciliationBatch, type ReconciliationBatch } from '@/api/reconciliation';
import { Play, X } from 'lucide-react';

const PAGE_SIZE = 20;

export default function ReconciliationBatchesPage() {
  return (
    <RoutePermissionGuard permissions={['payment.reconciliation.view']}>
      <BatchesContent />
    </RoutePermissionGuard>
  );
}

function BatchesContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [runOpen, setRunOpen] = useState(false);
  const [runType, setRunType] = useState('wallet_balances');

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['rec-batches', { skip, search, status: statusFilter, type: typeFilter }],
    queryFn: () => listReconciliationBatches({ skip, take: PAGE_SIZE, query: search || undefined, status: statusFilter || undefined, batchType: typeFilter || undefined }),
  });

  const runMutation = async () => {
    try {
      const r = await runReconciliationBatch({ batchType: runType });
      if (r.approvalRequestId) {
        toast.success('Approval request created', { description: `ID: ${r.approvalRequestId}` });
      } else {
        toast.success(r.message || 'Batch started');
      }
      queryClient.invalidateQueries({ queryKey: ['rec-batches'] });
    } catch (err: any) { toast.error(err.message || 'Run failed'); }
    setRunOpen(false);
  };

  const columns: DataTableColumn<ReconciliationBatch>[] = [
    { key: 'id', header: 'ID', render: (b) => <Link href={`/admin/security/reconciliation/batches/${b.id}`} className="font-mono text-xs text-primary hover:underline">{b.id.slice(0, 12)}…</Link> },
    { key: 'type', header: 'Type', render: (b) => <span className="text-xs capitalize">{b.batchType.replace(/_/g, ' ')}</span> },
    { key: 'status', header: 'Status', render: (b) => <StatusBadge status={b.status} /> },
    { key: 'total', header: 'Items', render: (b) => <span className="text-xs">{b.totalItems}</span> },
    { key: 'matched', header: 'Matched', render: (b) => <span className="text-xs text-green-600">{b.matchedCount}</span> },
    { key: 'mismatched', header: 'Mismatched', render: (b) => <span className="text-xs text-red-600">{b.mismatchedCount}</span> },
    { key: 'started', header: 'Started', render: (b) => <span className="text-xs">{b.startedAt ? new Date(b.startedAt).toLocaleString() : '—'}</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Reconciliation Batches</h1>
          <p className="text-sm text-muted-foreground">Automated and manual reconciliation runs</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <PermissionGuard permission="payment.reconciliation.run">
            <Button variant="outline" size="sm" onClick={() => setRunOpen(true)}><Play className="mr-1 h-3.5 w-3.5" /> Run</Button>
          </PermissionGuard>
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search..." className="w-64" />
        <select value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Types</option><option value="wallet_balances">Wallet Balances</option><option value="gateway_payments">Gateway Payments</option><option value="payment_wallet_posting">Wallet Posting</option><option value="withdrawal_payouts">Withdrawal Payouts</option><option value="ledger_hash_chain">Ledger Hash Chain</option><option value="full">Full</option>
        </select>
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option><option value="pending">Pending</option><option value="running">Running</option><option value="completed">Completed</option><option value="completed_with_issues">With Issues</option><option value="failed">Failed</option>
        </select>
      </FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(b) => b.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} onRowClick={(b) => window.location.href = `/admin/security/reconciliation/batches/${b.id}`} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {runOpen && (
        <ConfirmDialog open onClose={() => setRunOpen(false)} onConfirm={runMutation}
          title="Run Reconciliation"
          description={
            <div className="mt-3">
              <label className="text-xs font-medium">Batch Type</label>
              <select value={runType} onChange={(e) => setRunType(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                <option value="wallet_balances">Wallet Balances</option><option value="gateway_payments">Gateway Payments</option><option value="payment_wallet_posting">Wallet Posting</option><option value="withdrawal_payouts">Withdrawal Payouts</option><option value="ledger_hash_chain">Ledger Hash Chain</option>
              </select>
              {runType === 'full' && <p className="mt-2 text-xs text-red-600">Full reconciliation may take significant time and resources.</p>}
            </div>
          }
          confirmLabel="Run" />
      )}
    </div>
  );
}
