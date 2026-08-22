import type { ListPaymentIntentsParams } from '../types/payment-intent.types';

export const paymentIntentKeys = {
  all: ['payment-intents'] as const,
  lists: () => [...paymentIntentKeys.all, 'list'] as const,
  list: (params: ListPaymentIntentsParams) => [...paymentIntentKeys.lists(), params] as const,
  details: () => [...paymentIntentKeys.all, 'detail'] as const,
  detail: (id: string) => [...paymentIntentKeys.details(), id] as const,
  transactions: (id: string, params?: { skip?: number; take?: number }) =>
    [...paymentIntentKeys.all, 'transactions', id, params] as const,
};
