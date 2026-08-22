import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { approvalPolicyKeys } from './approval-policies.keys';
import { listApprovalPolicies, getApprovalPolicy, enableApprovalPolicy, disableApprovalPolicy, deleteApprovalPolicy } from './approval-policies.api';
import type { ListApprovalPoliciesParams } from '../types/approval-policies.types';

export function useApprovalPoliciesQuery(params: ListApprovalPoliciesParams) {
  return useQuery({
    queryKey: approvalPolicyKeys.list(params as Record<string, unknown>),
    queryFn: ({ signal }) => listApprovalPolicies(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useApprovalPolicyDetailQuery(policyId: string) {
  return useQuery({
    queryKey: approvalPolicyKeys.detail(policyId),
    queryFn: ({ signal }) => getApprovalPolicy(policyId, signal),
    enabled: !!policyId,
  });
}

export function useApprovalPolicyActionMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (req: { id: string; action: 'enable' | 'disable' | 'delete' }) => {
      switch (req.action) {
        case 'enable': return enableApprovalPolicy(req.id);
        case 'disable': return disableApprovalPolicy(req.id);
        case 'delete': return deleteApprovalPolicy(req.id);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: approvalPolicyKeys.all });
    },
  });
}
