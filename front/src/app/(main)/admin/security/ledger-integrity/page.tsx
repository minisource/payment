'use client';

import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DataTable, DataTableColumn, Pagination } from '@/components/shared/data-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { Loading } from '@/components/shared/states';
import { toast } from 'sonner';
import { listLedgerIntegrityChecks, verifyWalletHashChain, backfillWalletHashChain, runTenantDeepCheck, type LedgerIntegrityCheck } from '@/api/ledger-integrity';
import { Shield } from 'lucide-react';

const PAGE_SIZE = 20;

export default function LedgerIntegrityPage() {
  return (
    <RoutePermissionGuard permissions={['payment.security.reports.view_admin']}>
      <LedgerIntegrityContent />
    </RoutePermissionGuard>
  );
}

function LedgerIntegrityContent() {
  const queryClient = useQueryClient();
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [action, setAction] = useState<{ type: string; walletId?: string } | null>(null);

  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['ledger-integrity-checks', { skip, search }],
    queryFn: () => listLedgerIntegrityChecks({ skip, take: PAGE_SIZE, query: search || undefined }),
  });

  const handleAction = async () => {
    if (!action) return;
    try {
      if (action.type === 'verify-wallet' && action.walletId) {
        const r = await verifyWalletHashChain(action.walletId);
        toast.success(r.message || 'Verification complete');
      } else if (action.type === 'backfill' && action.walletId) {
        const r = await backfillWalletHashChain(action.walletId);
        toast.success(r.message || 'Backfill complete');
      } else if (action.type === 'deep-check') {
        const r = await runTenantDeepCheck();
        toast.success(r.message || 'Deep check started');
      }
      queryClient.invalidateQueries({ queryKey: ['ledger-integrity-checks'] });
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
    setAction(null);
  };

  const columns: DataTableColumn<LedgerIntegrityCheck>[] = [
    { key: 'id', header: 'ID', render: (c) => <span className="font-mono text-xs">{c.id.slice(0, 12)}…</span> },
    { key: 'wallet', header: 'Wallet', render: (c) => <Link href={`/admin/wallets/${c.walletId}`} className="text-primary hover:underline font-mono text-xs">{c.walletId.slice(0, 10)}…</Link> },
    { key: 'status', header: 'Status', render: (c) => <StatusBadge status={c.status} /> },
    { key: 'hash', header: 'Hash', render: (c) => <StatusBadge status={c.hashStatus} /> },
    { key: 'issues', header: 'Issues', render: (c) => <span className={`text-xs font-medium ${c.issueCount > 0 ? 'text-red-600' : 'text-green-600'}`}>{c.issueCount}</span> },
    { key: 'severity', header: 'Severity', render: (c) => <StatusBadge status={c.severity} /> },
    { key: 'created', header: 'Created', render: (c) => <span className="text-xs">{new Date(c.createdAt).toLocaleDateString()}</span> },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/security" className="text-sm text-muted-foreground hover:text-foreground">← Security</Link>
          <h1 className="text-2xl font-bold">Ledger Integrity</h1>
          <p className="text-sm text-muted-foreground">Hash chain verification, backfill, and deep checks</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <PermissionGuard permission="payment.ledger.verify_admin">
            <Button variant="outline" size="sm" onClick={() => setAction({ type: 'deep-check' })}><Shield className="mr-1 h-3.5 w-3.5" /> Deep Check</Button>
          </PermissionGuard>
        </div>
      </div>

      <Card>
        <CardContent className="p-4 bg-amber-50 dark:bg-amber-950/20 border rounded-lg">
          <p className="text-xs text-muted-foreground flex items-center gap-1"><Shield className="h-3.5 w-3.5" /> Ledger entries are append-only and cannot be edited or deleted. Integrity checks verify hash chains and detect tampering.</p>
        </CardContent>
      </Card>

      <FilterBar><SearchInput value={search} onChange={setSearch} placeholder="Search by wallet ID..." className="w-64" /></FilterBar>

      <Card><CardContent className="p-0">
        <DataTable columns={columns} data={data?.items || []} keyExtractor={(c) => c.id} isLoading={isLoading} isEmpty={!data?.items?.length} isError={isError} errorMessage={(error as Error)?.message} onRetry={() => refetch()} />
        {data && data.total > PAGE_SIZE && <Pagination skip={skip} take={PAGE_SIZE} total={data.total} onPageChange={setSkip} />}
      </CardContent></Card>

      {action && (
        <ConfirmDialog open onClose={() => setAction(null)} onConfirm={handleAction}
          title={action.type === 'deep-check' ? 'Run Tenant Deep Check' : action.type === 'backfill' ? 'Backfill Hash Chain' : 'Verify Wallet Hash Chain'}
          description={action.type === 'deep-check' ? 'Run a comprehensive ledger integrity check across the tenant. This may take several minutes.' : action.type === 'backfill' ? 'Backfill missing hash entries for this wallet. This is a sensitive operation that modifies ledger hash data.' : 'Verify the hash chain for this wallet to detect any tampering.'}
          confirmLabel={action.type === 'backfill' ? 'Backfill' : 'Run'} destructive={action.type === 'backfill'} />
      )}
    </div>
  );
}
