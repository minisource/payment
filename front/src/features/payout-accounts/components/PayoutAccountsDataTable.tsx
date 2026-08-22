'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { PayoutAccountItemDto } from '../types/payout-accounts.types';
import { payoutAccountColumns } from '../columns/payout-accounts.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type PayoutAccountsDataTableProps = Omit<DataTableProps<PayoutAccountItemDto>, 'columns'> & {
  columns?: ColumnDef<PayoutAccountItemDto>[];
};

export function PayoutAccountsDataTable({ columns, ...rest }: PayoutAccountsDataTableProps) {
  return (
    <DataTable<PayoutAccountItemDto>
      {...rest}
      columns={columns ?? payoutAccountColumns}
    />
  );
}
