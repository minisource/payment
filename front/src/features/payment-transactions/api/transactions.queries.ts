'use client';

import { useQuery } from '@tanstack/react-query';
import { transactionKeys } from './transactions.keys';
import { listPaymentTransactions, getPaymentTransaction } from './transactions.api';
import type { ListPaymentTransactionsParams } from '../types/transaction.types';

export function usePaymentTransactionsQuery(params: ListPaymentTransactionsParams) {
  return useQuery({
    queryKey: transactionKeys.list(params),
    queryFn: ({ signal }) => listPaymentTransactions(params, signal),
    placeholderData: (prev) => prev,
    staleTime: 15_000,
  });
}

export function usePaymentTransactionDetailQuery(transactionId: string | undefined) {
  return useQuery({
    queryKey: transactionKeys.detail(transactionId!),
    queryFn: ({ signal }) => getPaymentTransaction(transactionId!, signal),
    enabled: !!transactionId,
  });
}
