'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { WithdrawalsDataTable } from '@/features/withdrawals/components/WithdrawalsDataTable';
import { useWithdrawalsQuery } from '@/features/withdrawals/api/withdrawals.queries';

const PAGE_SIZE = 20;
const STATUSES = ['pending_review', 'more_info_required', 'approved', 'processing_payout', 'failed', 'paid', 'rejected'];

export default function WithdrawalsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.withdrawal.view_admin']}>
      <WithdrawalsContent />
    </RoutePermissionGuard>
  );
}

function WithdrawalsContent() {
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

  const { data, isLoading, isError, error, refetch, isFetching } = useWithdrawalsQuery(params);

  const handleSearchChange = useCallback((value: string) => { setSearch(value); setSkip(0); }, []);
  const handleStatusChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => { setStatusFilter(e.target.value); setSkip(0); }, []);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Withdrawals</h1>
          <p className="text-sm text-muted-foreground">All withdrawal requests</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isFetching} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={handleSearchChange} className="w-64" placeholder="Search withdrawals..." />
        <select value={statusFilter} onChange={handleStatusChange} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s.replace(/_/g, ' ')}</option>)}
        </select>
      </FilterBar>

      <div className="flex gap-2 flex-wrap">
        {STATUSES.map((s) => (
          <Link key={s} href={`/admin/withdrawals/queue?status=${s}`}
            className="rounded-full border px-3 py-1 text-xs hover:bg-accent transition-colors capitalize">{s.replace(/_/g, ' ')}</Link>
        ))}
        <Link href="/admin/withdrawals/queue" className="rounded-full border px-3 py-1 text-xs font-medium bg-primary/10 text-primary hover:bg-primary/20">
          Queue View →
        </Link>
      </div>

      <Card><CardContent className="p-0">
        <WithdrawalsDataTable
          data={data?.items || []}
          isLoading={isLoading}
          isEmpty={!data?.items?.length}
          emptyMessage="No withdrawals found"
          isError={isError}
          errorMessage={(error as Error)?.message}
          onRetry={() => refetch()}
          onRowClick={(w) => router.push(`/admin/withdrawals/${w.id}`)}
          skip={skip}
          take={PAGE_SIZE}
          total={data?.total}
          onPaginationChange={setSkip}
        />
      </CardContent></Card>
    </div>
  );
}
