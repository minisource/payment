import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { payoutAccountKeys } from './payout-accounts.keys';
import { listPayoutAccounts, getPayoutAccount, verifyPayoutAccount, rejectPayoutAccount, disablePayoutAccount } from './payout-accounts.api';
import type { ListPayoutAccountsParams } from '../types/payout-accounts.types';

export function usePayoutAccountsQuery(params: ListPayoutAccountsParams) {
  return useQuery({
    queryKey: payoutAccountKeys.list(params as Record<string, unknown>),
    queryFn: ({ signal }) => listPayoutAccounts(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function usePayoutAccountDetailQuery(payoutAccountId: string) {
  return useQuery({
    queryKey: payoutAccountKeys.detail(payoutAccountId),
    queryFn: ({ signal }) => getPayoutAccount(payoutAccountId, signal),
    enabled: !!payoutAccountId,
  });
}

export function usePayoutAccountActionMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (req: { id: string; action: 'verify' | 'reject' | 'disable'; reason?: string }) => {
      switch (req.action) {
        case 'verify': return verifyPayoutAccount(req.id);
        case 'reject': return rejectPayoutAccount(req.id, req.reason!);
        case 'disable': return disablePayoutAccount(req.id, req.reason!);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: payoutAccountKeys.all });
    },
  });
}
