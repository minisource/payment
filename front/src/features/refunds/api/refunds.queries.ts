import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { refundKeys } from './refunds.keys';
import * as api from './refunds.api';
import type { ListRefundsParams, CreateRefundRequest, ApproveRefundRequest, RejectRefundRequest } from '../types/refund.types';
import { toast } from 'sonner';
import { useTenantSegment } from '@/shared/tenant/use-tenant-segment';


export function useRefundsQuery(params: ListRefundsParams) {
  const tenant = useTenantSegment();
  return useQuery({
    queryKey: refundKeys.list(tenant, params as Record<string, unknown>),
    queryFn: ({ signal }) => api.listRefunds({
      ...params,
      tenant_id: tenant.tenantId ?? undefined,
      tenant_scope: tenant.tenantScope === 'all' ? 'all' : undefined,
    } as ListRefundsParams, signal),
  });
}

export function useRefundQuery(refundId: string | undefined) {
  const tenant = useTenantSegment();
  return useQuery({
    queryKey: refundKeys.detail(tenant, refundId!),
    queryFn: ({ signal }) => api.getRefund(refundId!, signal),
    enabled: !!refundId,
  });
}

export function useCreateRefundMutation() {
  const queryClient = useQueryClient();
  const tenant = useTenantSegment();
  return useMutation({
    mutationFn: (req: CreateRefundRequest) => api.createRefund(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: refundKeys.lists(tenant) });
      toast.success('Refund request created');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create refund');
    },
  });
}

export function useApproveRefundMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ refundId, request }: { refundId: string; request?: ApproveRefundRequest }) =>
      api.approveRefund(refundId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund approved');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to approve refund');
    },
  });
}

export function useRejectRefundMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ refundId, request }: { refundId: string; request: RejectRefundRequest }) =>
      api.rejectRefund(refundId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund rejected');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to reject refund');
    },
  });
}

export function useProcessRefundMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (refundId: string) => api.processRefund(refundId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund processing started');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to process refund');
    },
  });
}

export function useRetryRefundMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (refundId: string) => api.retryRefund(refundId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund retry started');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to retry refund');
    },
  });
}

export function useCancelRefundMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (refundId: string) => api.cancelRefund(refundId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund cancelled');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to cancel refund');
    },
  });
}

export function useMarkManualReviewMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ refundId, reason }: { refundId: string; reason: string }) =>
      api.markManualReview(refundId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['refunds'] });
      toast.success('Refund marked for manual review');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to mark for manual review');
    },
  });
}
