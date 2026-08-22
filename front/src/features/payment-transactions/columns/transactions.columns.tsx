'use client';

import Link from 'next/link';
import type { ColumnDef } from '@tanstack/react-table';
import type { PaymentTransactionDto } from '../types/transaction.types';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';

export const transactionColumns: ColumnDef<PaymentTransactionDto>[] = [
  {
    accessorKey: 'id',
    header: 'Transaction ID',
    cell: ({ row }) => (
      <Link
        href={`/admin/payment-transactions/${row.original.id}`}
        className="font-mono text-xs text-primary hover:underline"
      >
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    accessorKey: 'payment_intent_id',
    header: 'Intent',
    cell: ({ row }) => (
      <Link
        href={`/admin/payment-intents/${row.original.payment_intent_id}`}
        className="font-mono text-xs text-primary hover:underline"
      >
        {row.original.payment_intent_id.slice(0, 10)}…
      </Link>
    ),
  },
  {
    accessorKey: 'provider_code',
    header: 'Provider',
    cell: ({ row }) => <span className="text-xs">{row.original.provider_code}</span>,
  },
  {
    accessorKey: 'amount',
    header: 'Amount',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.amount} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'authority',
    header: 'Authority',
    cell: ({ row }) => (
      <span className="font-mono text-xs text-muted-foreground">
        {row.original.authority ? `${row.original.authority.slice(0, 12)}…` : '—'}
      </span>
    ),
  },
  {
    accessorKey: 'verified_at',
    header: 'Verified',
    cell: ({ row }) => (
      <span className="text-xs">
        {row.original.verified_at ? new Date(row.original.verified_at).toLocaleDateString() : '—'}
      </span>
    ),
  },
  {
    accessorKey: 'created_at',
    header: 'Date',
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleDateString()}</span>
    ),
  },
];
