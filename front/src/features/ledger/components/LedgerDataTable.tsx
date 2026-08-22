'use client';

import { DataTable } from '@/shared/components/data-table/DataTable';
import { ledgerEntryColumns } from '../columns/ledger.columns';
import type { LedgerEntryDto } from '../types/ledger.types';

interface LedgerDataTableProps {
  data: LedgerEntryDto[];
  total?: number;
  skip?: number;
  take?: number;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  onRowClick?: (item: LedgerEntryDto) => void;
  onPaginationChange?: (skip: number) => void;
}

export function LedgerDataTable({
  data,
  total,
  skip = 0,
  take = 25,
  isLoading,
  isEmpty,
  emptyMessage,
  isError,
  errorMessage,
  onRetry,
  onRowClick,
  onPaginationChange,
}: LedgerDataTableProps) {
  return (
    <DataTable<LedgerEntryDto>
      columns={ledgerEntryColumns}
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
