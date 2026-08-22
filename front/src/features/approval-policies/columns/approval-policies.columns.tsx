'use client';

import Link from 'next/link';
import { StatusBadge } from '@/components/shared/enhanced-badge';
import type { ColumnDef } from '@tanstack/react-table';
import type { ApprovalPolicyItemDto } from '../types/approval-policies.types';

export const approvalPolicyColumns: ColumnDef<ApprovalPolicyItemDto>[] = [
  {
    accessorKey: 'name',
    header: 'Name',
    cell: ({ row }) => (
      <Link href={`/admin/approval-policies/${row.original.id}`} className="text-xs font-medium text-primary hover:underline">
        {row.original.name}
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
    id: 'scope',
    header: 'Scope',
    cell: ({ row }) => (
      <span className="text-xs">
        {row.original.tenant_id ? 'Tenant' : row.original.application_code ? 'Application' : 'Global'}
      </span>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Status',
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: 'required_approvals',
    header: 'Required',
    cell: ({ row }) => <span className="text-xs font-mono">{row.original.required_approvals}</span>,
  },
  {
    id: 'threshold',
    header: 'Threshold',
    cell: ({ row }) => (
      <span className="text-xs">
        {row.original.amount_threshold ? `${row.original.amount_threshold} ${row.original.currency}` : 'Any'}
      </span>
    ),
  },
  {
    id: 'diffUser',
    header: 'Diff User',
    cell: ({ row }) => (
      <span className="text-xs">{row.original.require_different_user ? 'Yes' : 'No'}</span>
    ),
  },
];
