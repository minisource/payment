import { useQuery } from '@tanstack/react-query';
import { withdrawalKeys } from './withdrawals.keys';
import { listWithdrawals, getWithdrawal } from './withdrawals.api';
import type { ListWithdrawalsParams } from '../types/withdrawal.types';

export function useWithdrawalsQuery(params: ListWithdrawalsParams) {
  return useQuery({
    queryKey: withdrawalKeys.list(params),
    queryFn: ({ signal }) => listWithdrawals(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useWithdrawalDetailQuery(withdrawalId: string | undefined) {
  return useQuery({
    queryKey: withdrawalKeys.detail(withdrawalId ?? ''),
    queryFn: ({ signal }) => getWithdrawal(withdrawalId!, signal),
    enabled: !!withdrawalId,
  });
}
