'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { useGatewayProvidersQuery } from '@/features/gateways/api/gateways.queries';
import { GatewayProvidersDataTable } from '@/features/gateways/components/GatewayProvidersDataTable';

export default function GatewayProvidersPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.provider.view_admin']}>
      <ProvidersContent />
    </RoutePermissionGuard>
  );
}

function ProvidersContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');

  const { data, isLoading, isError, error, refetch } = useGatewayProvidersQuery({ query: search || undefined });

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Gateway Providers</h1>
          <p className="text-sm text-muted-foreground">Available payment gateway providers and their capabilities</p>
        </div>
        <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search providers..." className="w-64" />
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <GatewayProvidersDataTable
            data={data?.items || []}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No providers found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(p) => router.push(`/admin/gateways/providers/${p.code}`)}
          />
        </CardContent>
      </Card>
    </div>
  );
}
