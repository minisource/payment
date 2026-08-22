'use client';

import Link from 'next/link';
import type { ColumnDef } from '@tanstack/react-table';
import type { WalletDto } from '../types/wallet.types';
import { WalletStatusBadge, WalletOwnerCell, WalletBalanceCell } from '../components/WalletCells';

export interface WalletColumnsOptions {
  showActions?: boolean;
  onView?: (wallet: WalletDto) => void;
}

export const walletColumns: ColumnDef<WalletDto>[] = [
  {
    accessorKey: 'id',
    header: 'ID',
    enableSorting: true,
    cell: ({ row }) => (
      <Link href={`/admin/wallets/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    accessorKey: 'owner_id',
    header: 'Owner',
    cell: ({ row }) => <WalletOwnerCell wallet={row.original} />,
  },
  {
    accessorKey: 'available_balance',
    header: 'Balance',
    enableSorting: true,
    cell: ({ row }) => <WalletBalanceCell wallet={row.original} />,
  },
  {
    accessorKey: 'status',
    header: 'Status',
    enableSorting: true,
    cell: ({ row }) => <WalletStatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'created_at',
    header: 'Created',
    enableSorting: true,
    cell: ({ row }) => {
      const date = new Date(row.original.created_at);
      return <span className="text-xs text-muted-foreground">{date.toLocaleDateString('fa-IR')}</span>;
    },
  },
];

export function createWalletColumns(options?: WalletColumnsOptions): ColumnDef<WalletDto>[] {
  if (!options?.showActions) return walletColumns;

  return [
    ...walletColumns,
    {
      id: 'actions',
      header: '',
      cell: ({ row }) => (
        <button
          onClick={() => options?.onView?.(row.original)}
          className="text-xs text-primary hover:underline"
        >
          View
        </button>
      ),
    },
  ];
}
