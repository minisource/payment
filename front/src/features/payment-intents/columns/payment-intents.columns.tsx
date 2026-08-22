'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import type { ColumnDef } from '@tanstack/react-table';
import type { PaymentIntentDto } from '../types/payment-intent.types';

export const paymentIntentColumns: ColumnDef<PaymentIntentDto>[] = [
  {
    accessorKey: 'id',
    header: 'ID',
    enableSorting: true,
    cell: ({ row }) => (
      <Link href={`/admin/payment-intents/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    accessorKey: 'amount',
    header: 'Amount',
    enableSorting: true,
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.amount} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    enableSorting: true,
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'provider_code',
    header: 'Provider',
    cell: ({ row }) => <span className="text-xs">{row.original.provider_code || '—'}</span>,
  },
  {
    accessorKey: 'wallet_behavior',
    header: 'Wallet',
    cell: ({ row }) => (
      <span className="text-xs capitalize">{row.original.wallet_behavior.replace(/_/g, ' ')}</span>
    ),
  },
  {
    id: 'reference',
    header: 'Reference',
    cell: ({ row }) => {
      const pi = row.original;
      return pi.external_reference_type
        ? <span className="text-xs">{pi.external_reference_type}: {pi.external_reference_id?.slice(0, 12)}</span>
        : <span className="text-xs text-muted-foreground">—</span>;
    },
  },
  {
    accessorKey: 'created_at',
    header: 'Date',
    enableSorting: true,
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleDateString()}</span>
    ),
  },
];
