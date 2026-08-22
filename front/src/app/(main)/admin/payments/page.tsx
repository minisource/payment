'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, DateRangeFilter, RefreshButton, ExportButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { usePaymentIntentsQuery } from '@/features/payment-intents/api/payment-intents.queries';
import { PaymentIntentsDataTable } from '@/features/payment-intents/components/PaymentIntentsDataTable';
import type { PaymentIntentDto, PaymentIntentStatus } from '@/features/payment-intents/types/payment-intent.types';

const PAGE_SIZE = 20;

export default function PaymentIntentsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.intent.view_admin']}>
      <PaymentIntentsContent />
    </RoutePermissionGuard>
  );
}

function PaymentIntentsContent() {
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
    status: (statusFilter || undefined) as PaymentIntentStatus | undefined,
    dateFrom,
    dateTo,
  };

  const { data, isLoading, isError, error, refetch } = usePaymentIntentsQuery(params);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Payment Intents</h1>
          <p className="text-sm text-muted-foreground">All payment intents across tenants</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={(v) => { setSearch(v); setSkip(0); }} placeholder="Search by ID, reference..." className="w-72" />
        <DateRangeFilter dateFrom={dateFrom} dateTo={dateTo} onChange={(f, t) => { setDateFrom(f); setDateTo(t); setSkip(0); }} />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          <option value="pending">Pending</option>
          <option value="started">Started</option>
          <option value="requires_action">Requires Action</option>
          <option value="succeeded">Succeeded</option>
          <option value="failed">Failed</option>
          <option value="cancelled">Cancelled</option>
          <option value="expired">Expired</option>
        </select>
        <ExportButton disabled disabledReason="Export API not yet available" />
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <PaymentIntentsDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No payment intents found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(p) => router.push(`/admin/payment-intents/${p.id}`)}
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
