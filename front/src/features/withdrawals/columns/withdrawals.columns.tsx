'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import type { ColumnDef } from '@tanstack/react-table';
import type { WithdrawalItem } from '../types/withdrawal.types';

export const withdrawalColumns: ColumnDef<WithdrawalItem>[] = [
  {
    accessorKey: 'id',
    header: 'ID',
    cell: ({ row }) => (
      <Link href={`/admin/withdrawals/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
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
    accessorKey: 'fee_amount',
    header: 'Fee',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.fee_amount} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    accessorKey: 'net_amount',
    header: 'Net',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.net_amount} currency={row.original.currency} className="text-xs font-medium" />
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'created_at',
    header: 'Date',
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleDateString()}</span>
    ),
  },
];
