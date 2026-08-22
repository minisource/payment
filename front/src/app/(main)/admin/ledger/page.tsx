'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, DateRangeFilter, RefreshButton, ExportButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { useLedgerEntriesQuery } from '@/features/ledger/api/ledger.queries';
import { LedgerDataTable } from '@/features/ledger/components/LedgerDataTable';
import { Shield } from 'lucide-react';

const PAGE_SIZE = 25;

export default function LedgerExplorerPage() {
  return (
    <RoutePermissionGuard permissions={['payment.wallet.view_admin']}>
      <LedgerExplorerContent />
    </RoutePermissionGuard>
  );
}

function LedgerExplorerContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [dateFrom, setDateFrom] = useState<string>();
  const [dateTo, setDateTo] = useState<string>();
  const [entryType, setEntryType] = useState('');
  const [direction, setDirection] = useState('');

  const params = {
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    dateFrom,
    dateTo,
    entryType: entryType || undefined,
    direction: direction || undefined,
  };
  const { data, isLoading, isError, error, refetch } = useLedgerEntriesQuery(params);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Ledger Explorer</h1>
          <p className="text-sm text-muted-foreground">Immutable ledger entries across all wallets</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search by ID, wallet, reference..." className="w-72" />
        <DateRangeFilter dateFrom={dateFrom} dateTo={dateTo} onChange={(f, t) => { setDateFrom(f); setDateTo(t); setSkip(0); }} />
        <select value={entryType} onChange={(e) => { setEntryType(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Types</option>
          <option value="payment">Payment</option>
          <option value="withdrawal_lock">Withdrawal Lock</option>
          <option value="withdrawal_release">Withdrawal Release</option>
          <option value="withdrawal_capture">Withdrawal Capture</option>
          <option value="withdrawal_fee">Withdrawal Fee</option>
          <option value="refund">Refund</option>
          <option value="admin_credit">Admin Credit</option>
          <option value="admin_debit">Admin Debit</option>
        </select>
        <select value={direction} onChange={(e) => { setDirection(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Directions</option>
          <option value="credit">Credit</option>
          <option value="debit">Debit</option>
        </select>
        <ExportButton disabled disabledReason="Export API not yet available" />
      </FilterBar>

      <Card className="border-dashed bg-muted/20">
        <CardContent className="flex items-center gap-3 p-3">
          <Shield className="h-4 w-4 text-muted-foreground" />
          <p className="text-xs text-muted-foreground">Ledger entries are append-only and cannot be edited or deleted. Each entry contains a cryptographic hash for tamper-proof verification.</p>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="p-0">
          <LedgerDataTable
            data={data?.items || []}
            total={data?.total}
            skip={skip}
            take={PAGE_SIZE}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No ledger entries found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(e) => router.push(`/admin/ledger/${e.id}`)}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>
    </div>
  );
}
