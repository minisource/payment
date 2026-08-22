'use client';

import type { ColumnDef } from '@tanstack/react-table';
import type { ApprovalPolicyItemDto } from '../types/approval-policies.types';
import { approvalPolicyColumns } from '../columns/approval-policies.columns';
import { DataTable, type DataTableProps } from '@/shared/components/data-table/DataTable';

type ApprovalPoliciesDataTableProps = Omit<DataTableProps<ApprovalPolicyItemDto>, 'columns'> & {
  columns?: ColumnDef<ApprovalPolicyItemDto>[];
};

export function ApprovalPoliciesDataTable({ columns, ...rest }: ApprovalPoliciesDataTableProps) {
  return (
    <DataTable<ApprovalPolicyItemDto>
      {...rest}
      columns={columns ?? approvalPolicyColumns}
    />
  );
}
