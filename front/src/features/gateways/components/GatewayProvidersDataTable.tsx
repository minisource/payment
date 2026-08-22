'use client';

import { DataTable } from '@/shared/components/data-table/DataTable';
import { gatewayProviderColumns } from '../columns/gateway-providers.columns';
import type { GatewayProviderDetail } from '../types/gateway.types';

interface GatewayProvidersDataTableProps {
  data: GatewayProviderDetail[];
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  onRowClick?: (item: GatewayProviderDetail) => void;
}

export function GatewayProvidersDataTable({
  data,
  isLoading,
  isEmpty,
  emptyMessage,
  isError,
  errorMessage,
  onRetry,
  onRowClick,
}: GatewayProvidersDataTableProps) {
  return (
    <DataTable<GatewayProviderDetail>
      columns={gatewayProviderColumns}
      data={data}
      isLoading={isLoading}
      isEmpty={isEmpty}
      emptyMessage={emptyMessage}
      isError={isError}
      errorMessage={errorMessage}
      onRetry={onRetry}
      onRowClick={onRowClick}
    />
  );
}
