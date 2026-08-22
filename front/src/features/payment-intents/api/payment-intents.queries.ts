import { useQuery } from '@tanstack/react-query';
import { paymentIntentKeys } from './payment-intents.keys';
import { listPaymentIntents, getPaymentIntent, listPaymentIntentTransactions } from './payment-intents.api';
import type { ListPaymentIntentsParams } from '../types/payment-intent.types';

export function usePaymentIntentsQuery(params: ListPaymentIntentsParams) {
  return useQuery({
    queryKey: paymentIntentKeys.list(params),
    queryFn: ({ signal }) => listPaymentIntents(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function usePaymentIntentDetailQuery(paymentIntentId: string | undefined) {
  return useQuery({
    queryKey: paymentIntentKeys.detail(paymentIntentId ?? ''),
    queryFn: ({ signal }) => getPaymentIntent(paymentIntentId!, signal),
    enabled: !!paymentIntentId,
  });
}

export function usePaymentIntentTransactionsQuery(
  paymentIntentId: string | undefined,
  params?: { skip?: number; take?: number },
) {
  return useQuery({
    queryKey: paymentIntentKeys.transactions(paymentIntentId ?? '', params),
    queryFn: ({ signal }) => listPaymentIntentTransactions(paymentIntentId!, params, signal),
    enabled: !!paymentIntentId,
    placeholderData: (prev) => prev,
  });
}
