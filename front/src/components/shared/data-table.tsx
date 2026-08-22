/**
 * @deprecated Import from `@/shared/components/data-table/DataTable` instead.
 * This file is kept for backward compatibility during the frontend refactor.
 *
 * Provides a bridge component that converts old-style DataTableColumn<T>[]
 * to new-style ColumnDef<T>[] for unmigrated pages.
 */
'use client';

import { useMemo } from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { DataTable as NewDataTable } from '@/shared/components/data-table/DataTable';
import { DataTablePagination } from '@/shared/components/data-table/DataTablePagination';

/**
 * @deprecated Use `ColumnDef<T>` from `@tanstack/react-table` instead.
 */
export interface DataTableColumn<T> {
  key: string;
  header: string;
  render?: (item: T, index: number) => React.ReactNode;
  accessor?: (item: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
  sortable?: boolean;
}

interface LegacyDataTableProps<T> {
  columns: DataTableColumn<T>[];
  data: T[];
  keyExtractor: (item: T) => string;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  className?: string;
  compact?: boolean;
  onRowClick?: (item: T) => void;
  skip?: number;
  take?: number;
  total?: number;
  onPageChange?: (skip: number) => void;
}

/**
 * Bridge component: accepts old DataTableColumn<T>[] and converts to ColumnDef<T>[]
 * so unmigrated pages continue to work without changes.
 * @deprecated Use the new DataTable from `@/shared/components/data-table/DataTable` with ColumnDef directly.
 */
function LegacyDataTable<T>({ columns: legacyColumns, onPageChange, ...props }: LegacyDataTableProps<T>) {
  const newColumns = useMemo(() =>
    (legacyColumns as DataTableColumn<T>[]).map((col) => {
      const columnDef: ColumnDef<T> = {
        accessorKey: col.key,
        header: col.header,
        enableSorting: col.sortable ?? false,
      };

      if (col.render) {
        columnDef.cell = ({ row }: { row: { original: T; index: number } }) =>
          col.render!(row.original, row.index);
      } else if (col.accessor) {
        columnDef.cell = ({ row }: { row: { original: T } }) =>
          col.accessor!(row.original);
      }

      return columnDef;
    }),
    [legacyColumns],
  );

  return (
    <>
      <NewDataTable<T>
        {...(props as any)}
        columns={newColumns}
        skip={props.skip ?? 0}
        take={props.take ?? 20}
        total={props.total}
        onPaginationChange={onPageChange}
      />
    </>
  );
}

/** @deprecated Use `LegacyDataTable` above */
export const DataTable = LegacyDataTable;

export type DataTableProps<T> = LegacyDataTableProps<T>;

/** @deprecated Use `DataTablePagination` from `@/shared/components/data-table/DataTablePagination` */
export { DataTablePagination as Pagination };
