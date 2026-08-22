'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { ProviderBadge, GatewayHealthBadge } from '@/components/shared/gateway-badges';
import type { GatewayProviderDetail } from '../types/gateway.types';

export const gatewayProviderColumns: ColumnDef<GatewayProviderDetail>[] = [
  {
    accessorKey: 'code',
    header: 'Provider',
    cell: ({ row }) => <ProviderBadge provider={row.original.code} />,
  },
  {
    accessorKey: 'display_name',
    header: 'Name',
    cell: ({ row }) => <span className="text-xs font-medium">{row.original.display_name}</span>,
  },
  {
    id: 'currencies',
    header: 'Supported Currencies',
    cell: ({ row }) => (
      <div className="flex flex-wrap gap-0.5">
        {row.original.supported_currencies?.map((c) => (
          <span key={c} className="rounded bg-muted px-1.5 py-0.5 text-[10px] font-mono">{c}</span>
        ))}
      </div>
    ),
  },
  {
    id: 'capabilities',
    header: 'Capabilities',
    cell: ({ row }) => (
      <div className="flex flex-wrap gap-0.5">
        {row.original.supported_operations?.slice(0, 4).map((op) => (
          <span key={op} className="rounded bg-blue-50 dark:bg-blue-900/20 px-1.5 py-0.5 text-[10px]">{op}</span>
        ))}
      </div>
    ),
  },
  {
    accessorKey: 'enabled',
    header: 'Status',
    cell: ({ row }) => (
      <GatewayHealthBadge health={row.original.enabled ? 'healthy' : 'unknown'} />
    ),
  },
];
