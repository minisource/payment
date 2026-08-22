'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { PaymentIntentDto } from '../types/payment-intent.types';
import { paymentIntentColumns } from '../columns/payment-intents.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type PaymentIntentsDataTableProps = Omit<DataTableProps<PaymentIntentDto>, 'columns'> & {
  columns?: ColumnDef<PaymentIntentDto>[];
};

export function PaymentIntentsDataTable({ columns, ...rest }: PaymentIntentsDataTableProps) {
  return (
    <DataTable<PaymentIntentDto>
      {...rest}
      columns={columns ?? paymentIntentColumns}
    />
  );
}
