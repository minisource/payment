'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import type { ColumnDef } from '@tanstack/react-table';
import type { ApprovalRequestItemDto } from '../types/approvals.types';

export const approvalColumns: ColumnDef<ApprovalRequestItemDto>[] = [
  {
    id: 'id',
    header: 'ID',
    cell: ({ row }) => (
      <Link href={`/admin/approvals/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    id: 'operation',
    header: 'Operation',
    cell: ({ row }) => (
      <span className="text-xs capitalize">{row.original.operation_type.replace(/_/g, ' ')}</span>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    id: 'requester',
    header: 'Requester',
    cell: ({ row }) => (
      <span className="font-mono text-xs">{row.original.requested_by_user_id?.slice(0, 8)}</span>
    ),
  },
  {
    id: 'approvals',
    header: 'Approvals',
    cell: ({ row }) => (
      <span className="text-xs">{row.original.approvals_count} / {row.original.required_approvals}</span>
    ),
  },
  {
    id: 'expires',
    header: 'Expires',
    cell: ({ row }) => (
      <span className="text-xs">{row.original.expires_at ? new Date(row.original.expires_at).toLocaleDateString() : '—'}</span>
    ),
  },
  {
    id: 'created',
    header: 'Created',
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleDateString()}</span>
    ),
  },
];
