import type { ListWithdrawalsParams } from '../types/withdrawal.types';

export const withdrawalKeys = {
  all: ['withdrawals'] as const,
  lists: () => [...withdrawalKeys.all, 'list'] as const,
  list: (params: ListWithdrawalsParams) => [...withdrawalKeys.lists(), params] as const,
  details: () => [...withdrawalKeys.all, 'detail'] as const,
  detail: (id: string) => [...withdrawalKeys.details(), id] as const,
};
