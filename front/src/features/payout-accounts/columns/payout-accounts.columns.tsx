'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MaskedCardNumber, MaskedIban, MaskedAccountNumber } from '@/components/shared/masked-value';
import type { ColumnDef } from '@tanstack/react-table';
import type { PayoutAccountItemDto } from '../types/payout-accounts.types';

export const payoutAccountColumns: ColumnDef<PayoutAccountItemDto>[] = [
  {
    id: 'id',
    header: 'ID',
    cell: ({ row }) => (
      <Link href={`/admin/payout-accounts/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    id: 'owner',
    header: 'Owner',
    cell: ({ row }) => (
      <span className="text-xs">{row.original.owner_type}: {row.original.owner_id?.slice(0, 8)}</span>
    ),
  },
  {
    id: 'type',
    header: 'Type',
    cell: ({ row }) => (
      <span className="text-xs capitalize">{row.original.account_type.replace(/_/g, ' ')}</span>
    ),
  },
  {
    id: 'masked',
    header: 'Account',
    cell: ({ row }) => {
      const p = row.original;
      return (
        p.card_number_masked ? <MaskedCardNumber value={p.card_number_masked} /> :
        p.iban_masked ? <MaskedIban value={p.iban_masked} /> :
        <MaskedAccountNumber value={p.account_number_masked} />
      );
    },
  },
  {
    accessorKey: 'bank_name',
    header: 'Bank',
    cell: ({ row }) => <span className="text-xs">{row.original.bank_name || '—'}</span>,
  },
  {
    accessorKey: 'currency',
    header: 'Currency',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.currency}</span>,
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
];
