'use client';

import Link from 'next/link';
import type { ColumnDef } from '@tanstack/react-table';
import type { RefundRequestDto } from '../types/refund.types';

const statusColors: Record<string, string> = {
  Requested: 'bg-gray-100 text-gray-700',
  PendingReview: 'bg-yellow-100 text-yellow-700',
  Approved: 'bg-blue-100 text-blue-700',
  Rejected: 'bg-red-100 text-red-700',
  Cancelled: 'bg-gray-200 text-gray-500',
  HoldPending: 'bg-purple-100 text-purple-700',
  HoldCreated: 'bg-purple-100 text-purple-700',
  Processing: 'bg-indigo-100 text-indigo-700',
  GatewaySubmitted: 'bg-indigo-100 text-indigo-700',
  GatewaySucceeded: 'bg-green-100 text-green-700',
  GatewayFailed: 'bg-red-100 text-red-700',
  WalletDebitPending: 'bg-teal-100 text-teal-700',
  WalletDebited: 'bg-teal-100 text-teal-700',
  Completed: 'bg-green-200 text-green-800',
  Failed: 'bg-red-200 text-red-800',
  RequiresManualReview: 'bg-orange-100 text-orange-700',
};

function StatusBadge({ status }: { status: string }) {
  const colorClass = statusColors[status] ?? 'bg-gray-100 text-gray-700';
  return (
    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${colorClass}`}>
      {status}
    </span>
  );
}

export const refundColumns: ColumnDef<RefundRequestDto>[] = [
  {
    accessorKey: 'id',
    header: 'ID',
    cell: ({ row }) => (
      <Link href={`/admin/refunds/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    accessorKey: 'amount',
    header: 'Amount',
    cell: ({ row }) => (
      <span className="font-mono text-sm">
        {row.original.amount.toLocaleString()} {row.original.currency}
      </span>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'provider_code',
    header: 'Gateway',
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground">{row.original.provider_code}</span>
    ),
  },
  {
    accessorKey: 'reason',
    header: 'Reason',
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground max-w-[200px] truncate block">
        {row.original.reason}
      </span>
    ),
  },
  {
    accessorKey: 'created_at',
    header: 'Created',
    cell: ({ row }) => {
      const date = new Date(row.original.created_at);
      return <span className="text-xs text-muted-foreground">{date.toLocaleDateString('fa-IR')}</span>;
    },
  },
];
