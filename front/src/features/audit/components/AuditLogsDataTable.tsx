'use client';

import { DataTable } from '@/shared/components/data-table/DataTable';
import { auditLogColumns } from '../columns/audit.columns';
import type { AuditLogListItemDto } from '../types/audit.types';

interface AuditLogsDataTableProps {
  data: AuditLogListItemDto[];
  total?: number;
  skip?: number;
  take?: number;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  onRowClick?: (item: AuditLogListItemDto) => void;
  onPaginationChange?: (skip: number) => void;
}

export function AuditLogsDataTable({
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
}: AuditLogsDataTableProps) {
  return (
    <DataTable<AuditLogListItemDto>
      columns={auditLogColumns}
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
