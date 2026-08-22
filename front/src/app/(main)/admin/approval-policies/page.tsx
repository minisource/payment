'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { ConfirmDialog } from '@/components/shared/components';
import { toast } from 'sonner';
import { useApprovalPoliciesQuery, useApprovalPolicyActionMutation } from '@/features/approval-policies/api/approval-policies.queries';
import { ApprovalPoliciesDataTable } from '@/features/approval-policies/components/ApprovalPoliciesDataTable';

const PAGE_SIZE = 20;

export default function ApprovalPoliciesPage() {
  return (
    <RoutePermissionGuard permissions={['payment.admin_approval.policy.view']}>
      <PoliciesContent />
    </RoutePermissionGuard>
  );
}

function PoliciesContent() {
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [action, setAction] = useState<{ id: string; type: string } | null>(null);

  const { data, isLoading, isError, error, refetch } = useApprovalPoliciesQuery({
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
  });

  const actionMutation = useApprovalPolicyActionMutation();

  const handleAction = async () => {
    if (!action) return;
    try {
      await actionMutation.mutateAsync({ id: action.id, action: action.type as 'enable' | 'disable' | 'delete' });
      toast.success('Action completed');
      setAction(null);
    } catch (err: any) { toast.error(err.message || 'Action failed'); }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/admin/approvals" className="text-sm text-muted-foreground hover:text-foreground">← Approvals</Link>
          <h1 className="text-2xl font-bold">Approval Policies</h1>
          <p className="text-sm text-muted-foreground">Maker-checker rules: thresholds, required approvals, and conditions</p>
        </div>
        <div className="flex gap-2">
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
          <PermissionGuard permission="payment.admin_approval.policy.create">
            <Link href="/admin/approval-policies/new"><Button size="sm">New Policy</Button></Link>
          </PermissionGuard>
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={(v) => { setSearch(v); setSkip(0); }} placeholder="Search..." className="w-64" />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All</option>
          <option value="active">Active</option>
          <option value="disabled">Disabled</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <ApprovalPoliciesDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No approval policies found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
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
          onClose={() => setAction(null)}
          onConfirm={handleAction}
          title={`${action.type === 'enable' ? 'Enable' : action.type === 'disable' ? 'Disable' : 'Delete'} Policy`}
          description={action.type === 'delete' ? 'This will permanently delete this policy. This action cannot be undone.' : undefined}
          confirmLabel={action.type === 'enable' ? 'Enable' : action.type === 'disable' ? 'Disable' : 'Delete'}
          destructive={action.type !== 'enable'}
          confirmDisabled={actionMutation.isPending}
        />
      )}
    </div>
  );
}
