'use client';

import Link from 'next/link';
import type { ColumnDef } from '@tanstack/react-table';
import { MoneyAmount } from '@/components/shared/money-amount';
import type { LedgerEntryDto } from '../types/ledger.types';

export const ledgerEntryColumns: ColumnDef<LedgerEntryDto>[] = [
  {
    accessorKey: 'id',
    header: 'Entry ID',
    cell: ({ row }) => (
      <Link href={`/admin/ledger/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    id: 'wallet',
    header: 'Wallet',
    cell: ({ row }) => (
      <Link href={`/admin/wallets/${row.original.wallet_account_id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.wallet_account_id.slice(0, 10)}…
      </Link>
    ),
  },
  {
    accessorKey: 'entry_type',
    header: 'Type',
    cell: ({ row }) => (
      <span className="text-xs capitalize">{row.original.entry_type.replace(/_/g, ' ')}</span>
    ),
  },
  {
    accessorKey: 'direction',
    header: 'Dir',
    cell: ({ row }) => (
      <span className={
        row.original.direction === 'credit'
          ? 'text-green-600 text-xs font-medium'
          : 'text-red-600 text-xs font-medium'
      }>
        {row.original.direction.toUpperCase()}
      </span>
    ),
  },
  {
    accessorKey: 'amount',
    header: 'Amount',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.amount} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    id: 'balance_before',
    header: 'Before',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.balance_available_before} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    id: 'balance_after',
    header: 'After',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.balance_available_after} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    id: 'hash',
    header: 'Hash',
    cell: ({ row }) => (
      <span className="font-mono text-xs text-muted-foreground">
        {row.original.entry_hash?.slice(0, 10)}…
      </span>
    ),
  },
  {
    accessorKey: 'created_at',
    header: 'Date',
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleString()}</span>
    ),
  },
];
