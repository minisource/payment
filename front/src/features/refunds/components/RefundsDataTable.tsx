'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { RefundRequestDto } from '../types/refund.types';
import { refundColumns } from '../columns/refunds.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type RefundsDataTableProps = Omit<DataTableProps<RefundRequestDto>, 'columns'> & {
  columns?: ColumnDef<RefundRequestDto>[];
};

export function RefundsDataTable({ columns, ...rest }: RefundsDataTableProps) {
  return (
    <DataTable<RefundRequestDto>
      {...rest}
      columns={columns ?? refundColumns}
    />
  );
}
