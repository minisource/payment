'use client';

import { DataTable } from '@/shared/components/data-table/DataTable';
import { routingPolicyColumns } from '../columns/routing-policies.columns';
import type { RoutingPolicyListItem } from '../types/gateway.types';

interface RoutingPoliciesDataTableProps {
  data: RoutingPolicyListItem[];
  total?: number;
  skip?: number;
  take?: number;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  onRowClick?: (item: RoutingPolicyListItem) => void;
  onPaginationChange?: (skip: number) => void;
}

export function RoutingPoliciesDataTable({
  data,
  total,
  skip = 0,
  take = 20,
  isLoading,
  isEmpty,
  emptyMessage,
  isError,
  errorMessage,
  onRetry,
  onRowClick,
  onPaginationChange,
}: RoutingPoliciesDataTableProps) {
  return (
    <DataTable<RoutingPolicyListItem>
      columns={routingPolicyColumns}
      data={data}
      isLoading={isLoading}
      isEmpty={isEmpty}
      emptyMessage={emptyMessage}
      isError={isError}
      errorMessage={errorMessage}
      onRetry={onRetry}
      onRowClick={onRowClick}
      skip={skip}
      take={take}
      total={total}
      onPaginationChange={onPaginationChange}
    />
  );
}
