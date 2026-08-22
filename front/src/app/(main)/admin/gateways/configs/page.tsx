'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SearchInput, FilterBar, RefreshButton, ExportButton } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { GatewayConfigsDataTable } from '@/features/gateways/components/GatewayConfigsDataTable';
import { useGatewayConfigsQuery } from '@/features/gateways/api/gateways.queries';
import { Plus } from 'lucide-react';

const PAGE_SIZE = 20;

export default function GatewayConfigsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.config.view']}>
      <ConfigsContent />
    </RoutePermissionGuard>
  );
}

function ConfigsContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [statusFilter, setStatusFilter] = useState('');
  const [envFilter, setEnvFilter] = useState('');

  const params = {
    query: search || undefined,
    skip,
    take: PAGE_SIZE,
    status: statusFilter || undefined,
    environment: envFilter || undefined,
  };

  const { data, isLoading, isError, error, refetch, isFetching } = useGatewayConfigsQuery(params);

  const handleSearchChange = useCallback((value: string) => { setSearch(value); setSkip(0); }, []);
  const handleStatusChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => { setStatusFilter(e.target.value); setSkip(0); }, []);
  const handleEnvChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => { setEnvFilter(e.target.value); setSkip(0); }, []);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Gateway Configs</h1>
          <p className="text-sm text-muted-foreground">Manage payment gateway configurations</p>
        </div>
        <div className="flex items-center gap-2">
          <PermissionGuard permission="payment.gateway.config.manage">
            <Button size="sm" onClick={() => router.push('/admin/gateways/configs/new')}>
              <Plus className="mr-1 h-3.5 w-3.5" /> New Config
            </Button>
          </PermissionGuard>
          <RefreshButton onClick={() => refetch()} isLoading={isFetching} />
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={handleSearchChange} placeholder="Search configs..." className="w-64" />
        <select value={statusFilter} onChange={handleStatusChange} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Statuses</option>
          <option value="active">Active</option>
          <option value="disabled">Disabled</option>
          <option value="deleted">Deleted</option>
        </select>
        <select value={envFilter} onChange={handleEnvChange} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Environments</option>
          <option value="sandbox">Sandbox</option>
          <option value="production">Production</option>
        </select>
        <ExportButton disabled disabledReason="Export API not yet available" />
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <GatewayConfigsDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No configs found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(c) => router.push(`/admin/gateways/configs/${c.id}`)}
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
