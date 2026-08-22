import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { approvalKeys } from './approvals.keys';
import { listApprovalRequests, getApprovalRequest, approveApprovalRequest, rejectApprovalRequest, cancelApprovalRequest } from './approvals.api';
import type { ListApprovalRequestsParams } from '../types/approvals.types';

export function useApprovalsQuery(params: ListApprovalRequestsParams) {
  return useQuery({
    queryKey: approvalKeys.list(params as Record<string, unknown>),
    queryFn: ({ signal }) => listApprovalRequests(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useApprovalDetailQuery(approvalId: string) {
  return useQuery({
    queryKey: approvalKeys.detail(approvalId),
    queryFn: ({ signal }) => getApprovalRequest(approvalId, signal),
    enabled: !!approvalId,
  });
}

export function useApprovalActionMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (req: { id: string; action: 'approve' | 'reject' | 'cancel'; reason?: string }) => {
      switch (req.action) {
        case 'approve': return approveApprovalRequest(req.id);
        case 'reject': return rejectApprovalRequest(req.id, { reason: req.reason || 'Rejected' });
        case 'cancel': return cancelApprovalRequest(req.id, { reason: req.reason || 'Cancelled' });
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: approvalKeys.all });
    },
  });
}
