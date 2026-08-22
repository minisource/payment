'use client';

import { useState } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { FilterBar, RefreshButton, SearchInput } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { useAuditLogsQuery } from '@/features/audit/api/audit.queries';
import { AuditLogsDataTable } from '@/features/audit/components/AuditLogsDataTable';

const PAGE_SIZE = 20;

export default function AuditPage() {
  return (
    <RoutePermissionGuard permissions={['payment.audit.view_admin']}>
      <AuditContent />
    </RoutePermissionGuard>
  );
}

function AuditContent() {
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');
  const [actionFilter, setActionFilter] = useState('');

  const params = { skip, take: PAGE_SIZE, query: search || undefined, action: actionFilter || undefined };
  const { data, isLoading, isError, error, refetch } = useAuditLogsQuery(params);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Audit Logs</h1>
          <p className="text-sm text-muted-foreground">Tamper-evident audit trail with hash-chain verification</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search actor, action, entity..." className="w-80" />
        <select value={actionFilter} onChange={(e) => { setActionFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Actions</option>
          <option value="wallet.adjustment">Wallet Adjustment</option>
          <option value="withdrawal.approve">Withdrawal Approve</option>
          <option value="withdrawal.reject">Withdrawal Reject</option>
          <option value="gateway.config_update">Gateway Config Update</option>
          <option value="auth.login">Login</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <AuditLogsDataTable
            data={data?.items || []}
            total={data?.total}
            skip={skip}
            take={PAGE_SIZE}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No audit logs found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(a) => { window.location.href = `/admin/audit/${a.id}`; }}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>

      <p className="text-xs text-muted-foreground text-center">Audit logs are immutable. No edit or delete operations are available.</p>
    </div>
  );
}
