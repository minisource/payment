'use client';

import React from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  flexRender,
  type ColumnDef,
  type SortingState,
  type OnChangeFn,
} from '@tanstack/react-table';
import { cn } from '@/lib/utils';
import { ChevronDown, ChevronUp, ChevronsUpDown } from 'lucide-react';
import { DataTablePagination } from './DataTablePagination';

// ─── Props ───────────────────────────────────────────────

export interface DataTableProps<T> {
  columns: ColumnDef<T>[];
  data: T[];
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  emptyDescription?: string;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
  className?: string;
  compact?: boolean;
  onRowClick?: (item: T) => void;

  // Pagination (offset-based)
  skip?: number;
  take?: number;
  total?: number;
  onPaginationChange?: (skip: number) => void;

  // Sorting
  sorting?: SortingState;
  onSortingChange?: OnChangeFn<SortingState>;
}

// ─── Component ───────────────────────────────────────────

export function DataTable<T>({
  columns, data, isLoading, isEmpty,
  emptyMessage = 'No data found', emptyDescription,
  isError, errorMessage, onRetry,
  className, compact, onRowClick,
  skip = 0, take = 20, total, onPaginationChange,
  sorting, onSortingChange,
}: DataTableProps<T>) {
  const table = useReactTable<T>({
    data,
    columns,
    state: {
      sorting,
    },
    onSortingChange,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    manualSorting: true,
  });

  // ── Loading state ─────────────────────────────────────
  if (isLoading) {
    return (
      <div className={cn('rounded-md border', className)}>
        <div className="space-y-2 p-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-8 animate-pulse rounded bg-primary/10" />
          ))}
        </div>
      </div>
    );
  }

  // ── Error state ───────────────────────────────────────
  if (isError) {
    return (
      <div className={cn('flex min-h-[160px] flex-col items-center justify-center rounded-md border p-6 text-center', className)}>
        <p className="text-sm font-medium text-destructive">{errorMessage || 'Failed to load data'}</p>
        {onRetry && (
          <button onClick={onRetry} className="mt-2 text-xs text-primary hover:underline">
            Try again
          </button>
        )}
      </div>
    );
  }

  // ── Empty state ───────────────────────────────────────
  if (isEmpty || data.length === 0) {
    return (
      <div className={cn('flex min-h-[160px] flex-col items-center justify-center rounded-md border p-6 text-center', className)}>
        <p className="text-sm font-medium text-muted-foreground">{emptyMessage}</p>
        {emptyDescription && <p className="mt-1 text-xs text-muted-foreground">{emptyDescription}</p>}
      </div>
    );
  }

  const paddingY = compact ? 'py-1.5' : 'py-2.5';
  const textSize = compact ? 'text-xs' : 'text-sm';

  return (
    <div className={cn('overflow-x-auto rounded-md border', className)}>
      <table className="w-full">
        <thead>
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id} className="border-b bg-muted/50">
              {headerGroup.headers.map((header) => (
                <th
                  key={header.id}
                  className={cn('px-4 text-left font-medium text-muted-foreground', paddingY, textSize)}
                  colSpan={header.colSpan}
                >
                  {header.isPlaceholder ? null : (
                    <div
                      className={cn(
                        'flex items-center gap-1',
                        header.column.getCanSort() && 'cursor-pointer select-none hover:text-foreground',
                      )}
                      onClick={header.column.getToggleSortingHandler()}
                    >
                      {flexRender(header.column.columnDef.header, header.getContext())}
                      {{
                        asc: <ChevronUp className="h-3.5 w-3.5" />,
                        desc: <ChevronDown className="h-3.5 w-3.5" />,
                      }[header.column.getIsSorted() as string] ?? (
                        header.column.getCanSort() && <ChevronsUpDown className="h-3.5 w-3.5 opacity-30" />
                      )}
                    </div>
                  )}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr
              key={row.id}
              className={cn('border-b transition-colors hover:bg-muted/50', onRowClick && 'cursor-pointer')}
              onClick={() => onRowClick?.(row.original)}
            >
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id} className={cn('px-4', paddingY, textSize)}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>

      {total !== undefined && onPaginationChange && (
        <DataTablePagination
          skip={skip}
          take={take}
          total={total}
          onPageChange={onPaginationChange}
        />
      )}
    </div>
  );
}
