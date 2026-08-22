'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import { EnvironmentBadge, ScopeBadge, GatewayHealthBadge, ProviderBadge } from '@/components/shared/gateway-badges';
import type { ColumnDef } from '@tanstack/react-table';
import type { GatewayConfigListItem } from '../types/gateway.types';

export const gatewayConfigColumns: ColumnDef<GatewayConfigListItem>[] = [
  {
    accessorKey: 'name',
    header: 'Name',
    cell: ({ row }) => (
      <Link href={`/admin/gateways/configs/${row.original.id}`} className="text-xs font-medium text-primary hover:underline">
        {row.original.name}
      </Link>
    ),
  },
  {
    accessorKey: 'provider_code',
    header: 'Provider',
    cell: ({ row }) => <ProviderBadge provider={row.original.provider_code} />,
  },
  {
    accessorKey: 'scope',
    header: 'Scope',
    cell: ({ row }) => <ScopeBadge scope={row.original.scope} />,
  },
  {
    accessorKey: 'environment',
    header: 'Env',
    cell: ({ row }) => <EnvironmentBadge environment={row.original.environment} />,
  },
  {
    accessorKey: 'priority',
    header: 'Priority',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.priority}</span>,
  },
  {
    accessorKey: 'weight',
    header: 'Weight',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.weight}</span>,
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'health_status',
    header: 'Health',
    cell: ({ row }) => <GatewayHealthBadge health={row.original.health_status} />,
  },
  {
    accessorKey: 'supported_currencies',
    header: 'Currencies',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.supported_currencies.join(', ')}</span>,
  },
];
