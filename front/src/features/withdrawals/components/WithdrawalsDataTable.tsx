'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { WithdrawalItem } from '../types/withdrawal.types';
import { withdrawalColumns } from '../columns/withdrawals.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type WithdrawalsDataTableProps = Omit<DataTableProps<WithdrawalItem>, 'columns'> & {
  columns?: ColumnDef<WithdrawalItem>[];
};

export function WithdrawalsDataTable({ columns, ...rest }: WithdrawalsDataTableProps) {
  return (
    <DataTable<WithdrawalItem>
      {...rest}
      columns={columns ?? withdrawalColumns}
    />
  );
}
