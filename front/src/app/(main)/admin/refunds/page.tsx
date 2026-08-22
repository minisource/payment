'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { RefundsDataTable } from '@/features/refunds/components/RefundsDataTable';
import { useRefundsQuery } from '@/features/refunds/api/refunds.queries';
import type { RefundRequestDto } from '@/features/refunds/types/refund.types';
import type { ColumnDef } from '@tanstack/react-table';
import { Eye } from 'lucide-react';
import { Filter, Search, RefreshCw } from 'lucide-react';
import { t } from '@/shared/i18n/translations';

const PAGE_SIZE = 20;

function ActionsCell({ refundId }: { refundId: string }) {
  const router = useRouter();
  return (
    <Button variant="ghost" size="sm" onClick={() => router.push(`/admin/refunds/${refundId}`)} title="View details">
      <Eye className="h-3.5 w-3.5" />
    </Button>
  );
}

const actionsColumn: ColumnDef<RefundRequestDto> = {
  id: 'actions',
  header: '',
  cell: ({ row }) => <ActionsCell refundId={row.original.id} />,
};

export default function RefundsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.refund.view_admin']}>
      <RefundsContent />
    </RoutePermissionGuard>
  );
}

function RefundsContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');

  const params = {
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
  };

  const { data, isLoading, isError, error, refetch, isFetching } = useRefundsQuery(params);

  const handleSearchChange = useCallback((value: string) => {
    setSearch(value);
    setSkip(0);
  }, []);

  const columns = [actionsColumn];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Refunds</h1>
          <p className="text-sm text-muted-foreground">Manage refund requests and gateway operations</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isFetching}>
            <RefreshCw className={`h-3.5 w-3.5 mr-1 ${isFetching ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-3 rounded-lg border bg-card p-3">
        <div className="relative flex-1 min-w-[200px]">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <input
            type="text"
            value={search}
            onChange={(e) => handleSearchChange(e.target.value)}
            placeholder="Search by reason, gateway name..."
            className="h-9 w-full rounded-md border bg-background pl-9 pr-3 text-sm"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }}
          className="h-9 rounded-md border bg-background px-2 text-xs"
        >
          <option value="">All Statuses</option>
          <option value="Requested">Requested</option>
          <option value="PendingReview">Pending Review</option>
          <option value="Approved">Approved</option>
          <option value="HoldCreated">Hold Created</option>
          <option value="Processing">Processing</option>
          <option value="GatewaySubmitted">Gateway Submitted</option>
          <option value="GatewaySucceeded">Gateway Succeeded</option>
          <option value="Completed">Completed</option>
          <option value="Failed">Failed</option>
          <option value="RequiresManualReview">Manual Review</option>
        </select>
        <Filter className="h-3.5 w-3.5 text-muted-foreground" />
      </div>

      <Card>
        <CardContent className="p-0">
          <RefundsDataTable
            columns={columns}
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage={t('list.empty.refunds')}
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(r) => router.push(`/admin/refunds/${r.id}`)}
            skip={skip}
            take={PAGE_SIZE}
            total={data?.total}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>
    </div>
  );
}
