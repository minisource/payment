'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { SearchInput, FilterBar, RefreshButton } from '@/components/shared/filters';
import { RoutePermissionGuard, PermissionGuard } from '@/components/shared/permission-guards';
import { useRoutingPoliciesQuery } from '@/features/gateways/api/gateways.queries';
import { RoutingPoliciesDataTable } from '@/features/gateways/components/RoutingPoliciesDataTable';
import { Plus } from 'lucide-react';

const PAGE_SIZE = 20;

export default function RoutingPoliciesPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.routing.view_admin']}>
      <PoliciesContent />
    </RoutePermissionGuard>
  );
}

function PoliciesContent() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [skip, setSkip] = useState(0);
  const [strategyFilter, setStrategyFilter] = useState('');

  const params = { query: search || undefined, skip, take: PAGE_SIZE, strategy: strategyFilter || undefined };
  const { data, isLoading, isError, error, refetch } = useRoutingPoliciesQuery(params);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Routing Policies</h1>
          <p className="text-sm text-muted-foreground">Payment gateway routing strategies and rules</p>
        </div>
        <div className="flex items-center gap-2">
          <PermissionGuard permission="payment.gateway.routing.manage">
            <Button size="sm" onClick={() => router.push('/admin/gateways/routing-policies/new')}>
              <Plus className="mr-1 h-3.5 w-3.5" /> New Policy
            </Button>
          </PermissionGuard>
          <RefreshButton onClick={() => refetch()} isLoading={isLoading} />
        </div>
      </div>

      <FilterBar>
        <SearchInput value={search} onChange={setSearch} placeholder="Search policies..." className="w-64" />
        <select value={strategyFilter} onChange={(e) => { setStrategyFilter(e.target.value); setSkip(0); }} className="h-9 rounded-md border bg-background px-2 text-xs">
          <option value="">All Strategies</option>
          <option value="priority">Priority</option>
          <option value="random">Random</option>
          <option value="weighted_random">Weighted Random</option>
        </select>
      </FilterBar>

      <Card>
        <CardContent className="p-0">
          <RoutingPoliciesDataTable
            data={data?.items || []}
            total={data?.total}
            skip={skip}
            take={PAGE_SIZE}
            isLoading={isLoading}
            isEmpty={!data?.items?.length}
            emptyMessage="No policies found"
            isError={isError}
            errorMessage={(error as Error)?.message}
            onRetry={() => refetch()}
            onRowClick={(p) => router.push(`/admin/gateways/routing-policies/${p.id}`)}
            onPaginationChange={setSkip}
          />
        </CardContent>
      </Card>
    </div>
  );
}
