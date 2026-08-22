'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { usePayoutAccountsQuery, usePayoutAccountActionMutation } from '@/features/payout-accounts/api/payout-accounts.queries';
import { PayoutAccountsDataTable } from '@/features/payout-accounts/components/PayoutAccountsDataTable';

const PAGE_SIZE = 20;

export default function PayoutAccountsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.payout_account.view_admin']}>
      <PayoutAccountsContent />
    </RoutePermissionGuard>
  );
}

function PayoutAccountsContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');
  const [actionState, setActionState] = useState<{ id: string; action: 'verify' | 'reject' | 'disable' } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = usePayoutAccountsQuery({
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
  });

  const actionMutation = usePayoutAccountActionMutation();

  const handleAction = async () => {
    if (!actionState) return;
    try {
      await actionMutation.mutateAsync({ ...actionState, reason: reason || undefined });
      toast.success('Action completed');
      setActionState(null);
      setReason('');
    } catch (err: any) {
      toast.error(err.message || 'Action failed');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Payout Accounts</h1>
          <p className="text-sm text-muted-foreground">User payout accounts (cards, IBANs, bank accounts)</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={(v) => { setSearch(v); setSkip(0); }} placeholder="Search accounts..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option>
          <option value="pending_verification">Pending</option>
          <option value="verified">Verified</option>
          <option value="rejected">Rejected</option>
          <option value="disabled">Disabled</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <PayoutAccountsDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No payout accounts found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(p) => router.push(`/admin/payout-accounts/${p.id}`)}
            skip={skip}
            take={PAGE_SIZE}
            total={data?.total}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>

      {actionState && (
        <ConfirmDialog
          open
          onClose={() => { setActionState(null); setReason(''); }}
          onConfirm={handleAction}
          confirmDisabled={actionState.action !== 'verify' && !reason}
          title={`${actionState.action === 'verify' ? 'Verify' : actionState.action === 'reject' ? 'Reject' : 'Disable'} Payout Account`}
          description={actionState.action !== 'verify' ? (
            <div className="mt-3">
              <label className="text-xs font-medium">Reason *</label>
              <input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="Enter reason..." />
            </div>
          ) : undefined}
          confirmLabel={actionState.action === 'verify' ? 'Verify' : actionState.action === 'reject' ? 'Reject' : 'Disable'}
          destructive={actionState.action !== 'verify'}
        />
      )}
    </div>
  );
}
