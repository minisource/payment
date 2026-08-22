'use client';

import Link from 'next/link';
import { Copy } from 'lucide-react';
import { toast } from 'sonner';
import type { ColumnDef } from '@tanstack/react-table';
import type { AuditLogListItemDto } from '../types/audit.types';

const HASH_STATUS_CLASSES: Record<string, string> = {
  valid: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  invalid: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  unverified: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400',
  not_applicable: 'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400',
};

function HashStatusBadge({ status }: { status: string }) {
  return (
    <span className={`rounded px-2 py-0.5 text-xs font-medium ${HASH_STATUS_CLASSES[status] || ''}`}>
      {status.replace(/_/g, ' ')}
    </span>
  );
}

function CopyButton({ text }: { text: string }) {
  return (
    <button
      onClick={(ev) => { ev.stopPropagation(); navigator.clipboard.writeText(text); toast.success('Request ID copied'); }}
      className="font-mono text-xs text-muted-foreground hover:text-primary"
    >
      <Copy className="inline h-3 w-3 mr-1" />
      {text.slice(0, 8)}…
    </button>
  );
}

export const auditLogColumns: ColumnDef<AuditLogListItemDto>[] = [
  {
    accessorKey: 'id',
    header: 'Audit ID',
    cell: ({ row }) => (
      <Link href={`/admin/audit/${row.original.id}`} className="font-mono text-xs text-primary hover:underline">
        {row.original.id.slice(0, 12)}…
      </Link>
    ),
  },
  {
    id: 'actor',
    header: 'Actor',
    cell: ({ row }) => (
      <span className="text-xs">
        {row.original.actor_type}{row.original.actor_user_id ? `: ${row.original.actor_user_id.slice(0, 8)}` : ''}
      </span>
    ),
  },
  {
    accessorKey: 'action',
    header: 'Action',
    cell: ({ row }) => <span className="text-xs font-mono font-medium">{row.original.action}</span>,
  },
  {
    id: 'entity',
    header: 'Entity',
    cell: ({ row }) => (
      <span className="font-mono text-xs">
        {row.original.entity_type}/{row.original.entity_id?.slice(0, 8)}
      </span>
    ),
  },
  {
    id: 'request_id',
    header: 'Request ID',
    cell: ({ row }) =>
      row.original.request_id ? (
        <CopyButton text={row.original.request_id} />
      ) : (
        <span className="text-xs text-muted-foreground">—</span>
      ),
  },
  {
    id: 'hash_status',
    header: 'Hash',
    cell: ({ row }) => <HashStatusBadge status={row.original.hash_status} />,
  },
  {
    accessorKey: 'created_at',
    header: 'Date',
    cell: ({ row }) => <span className="text-xs">{new Date(row.original.created_at).toLocaleString()}</span>,
  },
];
