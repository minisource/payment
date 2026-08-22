'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { PaymentLinkItemDto } from '../types/payment-links.types';
import { paymentLinkColumns } from '../columns/payment-links.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type PaymentLinksDataTableProps = Omit<DataTableProps<PaymentLinkItemDto>, 'columns'> & {
  columns?: ColumnDef<PaymentLinkItemDto>[];
};

export function PaymentLinksDataTable({ columns, ...rest }: PaymentLinksDataTableProps) {
  return (
    <DataTable<PaymentLinkItemDto>
      {...rest}
      columns={columns ?? paymentLinkColumns}
    />
  );
}
