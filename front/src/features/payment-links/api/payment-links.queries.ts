import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { paymentLinkKeys } from './payment-links.keys';
import { listPaymentLinks, getPaymentLink, pausePaymentLink, resumePaymentLink, disablePaymentLink } from './payment-links.api';
import type { ListPaymentLinksParams } from '../types/payment-links.types';

export function usePaymentLinksQuery(params: ListPaymentLinksParams) {
  return useQuery({
    queryKey: paymentLinkKeys.list(params as Record<string, unknown>),
    queryFn: ({ signal }) => listPaymentLinks(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function usePaymentLinkDetailQuery(paymentLinkId: string) {
  return useQuery({
    queryKey: paymentLinkKeys.detail(paymentLinkId),
    queryFn: ({ signal }) => getPaymentLink(paymentLinkId, signal),
    enabled: !!paymentLinkId,
  });
}

export function usePaymentLinkToggleMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (req: { id: string; action: 'pause' | 'resume' | 'disable'; reason?: string }) => {
      switch (req.action) {
        case 'pause': return pausePaymentLink(req.id);
        case 'resume': return resumePaymentLink(req.id);
        case 'disable': return disablePaymentLink(req.id, req.reason);
      }
    },
    onSuccess: () => {
      // Invalidate all payment-links caches (lists + details) to match old page behavior
      queryClient.invalidateQueries({ queryKey: paymentLinkKeys.all });
    },
  });
}
