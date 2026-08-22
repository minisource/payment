'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { ApprovalRequestItemDto } from '../types/approvals.types';
import { approvalColumns } from '../columns/approvals.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type ApprovalsDataTableProps = Omit<DataTableProps<ApprovalRequestItemDto>, 'columns'> & {
  columns?: ColumnDef<ApprovalRequestItemDto>[];
};

export function ApprovalsDataTable({ columns, ...rest }: ApprovalsDataTableProps) {
  return (
    <DataTable<ApprovalRequestItemDto>
      {...rest}
      columns={columns ?? approvalColumns}
    />
  );
}
