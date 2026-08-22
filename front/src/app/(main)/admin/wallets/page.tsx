'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { t } from '@/shared/i18n/translations';
import { SearchInput, FilterBar, RefreshButton, ExportButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { WalletsDataTable } from '@/features/wallets/components/WalletsDataTable';
import { useWalletsQuery } from '@/features/wallets/api/wallets.queries';
import { walletColumns } from '@/features/wallets/columns/wallets.columns';
import type { ColumnDef } from '@tanstack/react-table';
import type { WalletDto } from '@/features/wallets/types/wallet.types';
import { Eye, Copy } from 'lucide-react';

const PAGE_SIZE = 20;

function WalletActions({ walletId }: { walletId: string }) {
  const router = useRouter();
  return (
    <div className="flex items-center gap-1">
      <Button variant="ghost" size="sm" onClick={() => router.push(`/admin/wallets/${walletId}`)} title="View details">
        <Eye className="h-3.5 w-3.5" />
      </Button>
      <Button variant="ghost" size="sm" onClick={() => navigator.clipboard.writeText(walletId)} title="Copy ID">
        <Copy className="h-3.5 w-3.5" />
      </Button>
    </div>
  );
}

function ActionsCell({ row }: { row: { original: WalletDto } }) {
  return <WalletActions walletId={row.original.id} />;
}

const actionsColumn: ColumnDef<WalletDto> = {
  id: 'actions',
  header: '',
  cell: ({ row }) => <ActionsCell row={row} />,
};

const pageColumns: ColumnDef<WalletDto>[] = [...walletColumns, actionsColumn];

export default function WalletsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.wallet.view_admin']}>
      <WalletsContent />
    </RoutePermissionGuard>
  );
}

function WalletsContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');
  const [currencyFilter, setCurrencyFilter] = useState('');

  const params = {
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: (statusFilter || undefined) as WalletDto['status'] | undefined,
    currency: currencyFilter || undefined,
  };

  const { data, isLoading, isError, error, refetch, isFetching } = useWalletsQuery(params);

  const handleStatusChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setStatusFilter(e.target.value);
    setSkip(0);
  }, []);

  const handleCurrencyChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setCurrencyFilter(e.target.value);
    setSkip(0);
  }, []);

  const handleSearchChange = useCallback((value: string) => {
    setSearch(value);
    setSkip(0);
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Wallets</h1>
          <p className="text-sm text-muted-foreground">Manage and monitor all tenant wallets</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isFetching} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={handleSearchChange} placeholder="Search by wallet ID or owner..." className="w-72" />
        <select value={statusFilter} onChange={handleStatusChange} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          <option value="active">Active</option>
          <option value="frozen">Frozen</option>
          <option value="disabled">Disabled</option>
          <option value="deleted">Deleted</option>
        </select>
        <select value={currencyFilter} onChange={handleCurrencyChange} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Currencies</option>
          <option value="IRT">IRT</option>
          <option value="IRR">IRR</option>
          <option value="USD">USD</option>
        </select>
        <ExportButton disabled disabledReason="Export API not yet available" />
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <WalletsDataTable
            columns={pageColumns}
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage={t('list.empty.wallets')}
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(w) => router.push(`/admin/wallets/${w.id}`)}
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
