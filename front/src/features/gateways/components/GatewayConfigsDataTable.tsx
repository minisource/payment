'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { GatewayConfigListItem } from '../types/gateway.types';
import { gatewayConfigColumns } from '../columns/gateway-configs.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type GatewayConfigsDataTableProps = Omit<DataTableProps<GatewayConfigListItem>, 'columns'> & {
  columns?: ColumnDef<GatewayConfigListItem>[];
};

export function GatewayConfigsDataTable({ columns, ...rest }: GatewayConfigsDataTableProps) {
  return (
    <DataTable<GatewayConfigListItem>
      {...rest}
      columns={columns ?? gatewayConfigColumns}
    />
  );
}
