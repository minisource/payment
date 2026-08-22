'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { MoneyAmount } from '@/components/shared/money-amount';
import { Copy } from 'lucide-react';
import { toast } from 'sonner';
import type { ColumnDef } from '@tanstack/react-table';
import type { PaymentLinkItemDto } from '../types/payment-links.types';

export const paymentLinkColumns: ColumnDef<PaymentLinkItemDto>[] = [
  {
    accessorKey: 'title',
    header: 'Title',
    cell: ({ row }) => (
      <Link href={`/admin/payment-links/${row.original.id}`} className="text-xs font-medium text-primary hover:underline">
        {row.original.title}
      </Link>
    ),
  },
  {
    id: 'amount',
    header: 'Amount',
    cell: ({ row }) =>
      row.original.amount != null ? (
        <MoneyAmount amount={row.original.amount} currency={row.original.currency} className="text-xs" />
      ) : (
        <span className="text-xs text-muted-foreground">{row.original.amount_type}</span>
      ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    id: 'payments',
    header: 'Payments',
    cell: ({ row }) => (
      <span className="text-xs">{row.original.used_count}{row.original.max_uses ? `/${row.original.max_uses}` : ''}</span>
    ),
  },
  {
    id: 'totalPaid',
    header: 'Total Paid',
    cell: ({ row }) => (
      <MoneyAmount amount={row.original.total_paid} currency={row.original.currency} className="text-xs" />
    ),
  },
  {
    id: 'url',
    header: 'Public URL',
    cell: ({ row }) =>
      row.original.safe_public_url ? (
        <button
          onClick={(e) => { e.stopPropagation(); navigator.clipboard.writeText(row.original.safe_public_url!); toast.success('URL copied'); }}
          className="text-xs text-primary hover:underline"
        >
          <Copy className="inline h-3 w-3" /> Copy
        </button>
      ) : (
        <span className="text-xs text-muted-foreground">—</span>
      ),
  },
];
