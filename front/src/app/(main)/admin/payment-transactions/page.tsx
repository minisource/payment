'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, DateRangeFilter, RefreshButton, ExportButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { usePaymentTransactionsQuery } from '@/features/payment-transactions/api/transactions.queries';
import { TransactionsDataTable } from '@/features/payment-transactions/components/TransactionsDataTable';

const PAGE_SIZE = 20;

export default function PaymentTransactionsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.intent.view_admin']}>
      <PaymentTransactionsContent />
    </RoutePermissionGuard>
  );
}

function PaymentTransactionsContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');
  const [dateFrom, setDateFrom] = useState<string>();
  const [dateTo, setDateTo] = useState<string>();

  const params = {
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
    dateFrom,
    dateTo,
  };
  const { data, isLoading, isError, error, refetch } = usePaymentTransactionsQuery(params);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Gateway Transactions</h1>
          <p className="text-sm text-muted-foreground">All gateway payment transactions</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search by ID, authority, reference..." className="w-72" />
        <DateRangeFilter dateFrom={dateFrom} dateTo={dateTo} onChange={(f, t) => { setDateFrom(f); setDateTo(t); setSkip(0); }} />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          <option value="pending">Pending</option>
          <option value="redirect_required">Redirect Required</option>
          <option value="callback_received">Callback Received</option>
          <option value="verified">Verified</option>
          <option value="failed">Failed</option>
          <option value="cancelled">Cancelled</option>
          <option value="expired">Expired</option>
        </select>
        <ExportButton disabled disabledReason="Export API not yet available" />
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <TransactionsDataTable
            data={data?.items || []}
            total={data?.total}
            skip={skip}
            take={PAGE_SIZE}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No transactions found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(t) => router.push(`/admin/payment-transactions/${t.id}`)}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>
    </div>
  );
}
