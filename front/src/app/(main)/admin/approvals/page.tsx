'use client';

import { useState } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { useApprovalsQuery, useApprovalActionMutation } from '@/features/approvals/api/approvals.queries';
import { ApprovalsDataTable } from '@/features/approvals/components/ApprovalsDataTable';


const PAGE_SIZE = 20;

export default function ApprovalsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.admin_approval.view']}>
      <ApprovalsContent />
    </RoutePermissionGuard>
  );
}

function ApprovalsContent() {
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string; reason?: string } | null>(null);
  const [reason, setReason] = useState('');

  const { data, isLoading, isError, error, refetch } = useApprovalsQuery({
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
  });

  const actionMutation = useApprovalActionMutation();

  const handleAction = async () => {
    if (!action) return;
    try {
      await actionMutation.mutateAsync({ id: action.id, action: action.type as 'approve' | 'reject' | 'cancel', reason: reason || undefined });
      toast.success('Action completed');
      setReason('');
      setAction(null);
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Admin Approvals</h1>
          <p className="text-sm text-muted-foreground">Maker-checker workflow for sensitive financial operations</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <a href="/admin/approval-policies"><Button variant="outline" size="sm">Policies</Button></a>
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={(v) => { setSearch(v); setSkip(0); }} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option>
          <option value="pending">Pending</option>
          <option value="approved">Approved</option>
          <option value="rejected">Rejected</option>
          <option value="expired">Expired</option>
          <option value="executed">Executed</option>
          <option value="failed">Failed</option>
          <option value="cancelled">Cancelled</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <ApprovalsDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No approval requests found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(a) => window.location.href = `/admin/approvals/${a.id}`}
            skip={skip}
            take={PAGE_SIZE}
            total={data?.total}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>

      {action && (
        <ConfirmDialog
          open
          onClose={() => { setAction(null); setReason(''); }}
          onConfirm={handleAction}
          confirmDisabled={actionMutation.isPending || ((action.type === 'reject' || action.type === 'cancel') && !reason)}
          title={`${action.type === 'approve' ? 'Approve' : action.type === 'reject' ? 'Reject' : 'Cancel'} Request`}
          description={action.type !== 'approve' ? (
            <div className="mt-3">
              <label className="text-xs font-medium">Reason *</label>
              <input value={reason} onChange={(e) => setReason(e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="Enter reason..." />
            </div>
          ) : 'Approve this request and proceed with execution?'}
          confirmLabel={action.type === 'approve' ? 'Approve' : action.type === 'reject' ? 'Reject' : 'Cancel'}
          destructive={action.type !== 'approve'}
        />
      )}
    </div>
  );
}
