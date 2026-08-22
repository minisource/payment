import type { ListPaymentTransactionsParams } from '../types/transaction.types';

export const transactionKeys = {
  all: ['payment-transactions'] as const,
  lists: () => [...transactionKeys.all, 'list'] as const,
  list: (params: ListPaymentTransactionsParams) => [...transactionKeys.lists(), params] as const,
  details: () => [...transactionKeys.all, 'detail'] as const,
  detail: (id: string) => [...transactionKeys.details(), id] as const,
};
