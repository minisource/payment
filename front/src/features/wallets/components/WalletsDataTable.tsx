'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { WalletDto } from '../types/wallet.types';
import { walletColumns } from '../columns/wallets.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type WalletsDataTableProps = Omit<DataTableProps<WalletDto>, 'columns'> & {
  /** Optional override for default columns. Use to add page-specific action columns. */
  columns?: ColumnDef<WalletDto>[];
};

export function WalletsDataTable({ columns, ...rest }: WalletsDataTableProps) {
  return (
    <DataTable<WalletDto>
      {...rest}
      columns={columns ?? walletColumns}
    />
  );
}
