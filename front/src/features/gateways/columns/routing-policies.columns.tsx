'use client';

import Link from 'next/link';
import type { ColumnDef } from '@tanstack/react-table';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { StrategyBadge, ScopeBadge } from '@/components/shared/gateway-badges';
import type { RoutingPolicyListItem } from '../types/gateway.types';

export const routingPolicyColumns: ColumnDef<RoutingPolicyListItem>[] = [
  {
    accessorKey: 'name',
    header: 'Name',
    cell: ({ row }) => (
      <Link
        href={`/admin/gateways/routing-policies/${row.original.id}`}
        className="text-xs font-medium text-primary hover:underline"
      >
        {row.original.name}
      </Link>
    ),
  },
  {
    accessorKey: 'scope',
    header: 'Scope',
    cell: ({ row }) => <ScopeBadge scope={row.original.scope} />,
  },
  {
    accessorKey: 'strategy',
    header: 'Strategy',
    cell: ({ row }) => <StrategyBadge strategy={row.original.strategy} />,
  },
  {
    accessorKey: 'fallback_enabled',
    header: 'Fallback',
    cell: ({ row }) =>
      row.original.fallback_enabled ? (
        <span className="text-xs text-green-600 font-medium">Yes</span>
      ) : (
        <span className="text-xs text-muted-foreground">No</span>
      ),
  },
  {
    accessorKey: 'rules_count',
    header: 'Rules',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.rules_count}</span>,
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'created_at',
    header: 'Created',
    cell: ({ row }) => (
      <span className="text-xs">{new Date(row.original.created_at).toLocaleDateString()}</span>
    ),
  },
];
