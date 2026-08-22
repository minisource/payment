import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { walletKeys } from './wallets.keys';
import {
  listWallets,
  getWallet,
  listWalletTransactions,
  listWalletLedger,
  creditWallet,
  debitWallet,
  verifyWalletHashChain,
  backfillWalletHashChain,
  runWalletDeepCheck,
} from './wallets.api';
import type { ListWalletsParams, ListWalletTransactionsParams, WalletAdjustmentRequest } from '../types/wallet.types';

// ─── Queries ─────────────────────────────────────────────

export function useWalletsQuery(params: ListWalletsParams) {
  return useQuery({
    queryKey: walletKeys.list(params),
    queryFn: ({ signal }) => listWallets(params, signal),
    placeholderData: (prev) => prev,
  });
}

export function useWalletDetailQuery(walletId: string | undefined) {
  return useQuery({
    queryKey: walletKeys.detail(walletId ?? ''),
    queryFn: ({ signal }) => getWallet(walletId!, signal),
    enabled: !!walletId,
  });
}

export function useWalletTransactionsQuery(walletId: string | undefined, params?: ListWalletTransactionsParams) {
  return useQuery({
    queryKey: walletKeys.transactions(walletId ?? '', params),
    queryFn: ({ signal }) => listWalletTransactions(walletId!, params, signal),
    enabled: !!walletId,
    placeholderData: (prev) => prev,
  });
}

export function useWalletLedgerQuery(walletId: string | undefined, params?: { skip?: number; take?: number }) {
  return useQuery({
    queryKey: walletKeys.ledger(walletId ?? '', params),
    queryFn: ({ signal }) => listWalletLedger(walletId!, params, signal),
    enabled: !!walletId,
    placeholderData: (prev) => prev,
  });
}

// ─── Mutations ───────────────────────────────────────────

export function useCreditWalletMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ walletId, data }: { walletId: string; data: WalletAdjustmentRequest }) =>
      creditWallet(walletId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: walletKeys.all });
    },
  });
}

export function useDebitWalletMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ walletId, data }: { walletId: string; data: WalletAdjustmentRequest }) =>
      debitWallet(walletId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: walletKeys.all });
    },
  });
}

export function useVerifyHashChainMutation() {
  return useMutation({
    mutationFn: (walletId: string) => verifyWalletHashChain(walletId),
  });
}

export function useBackfillHashChainMutation() {
  return useMutation({
    mutationFn: (walletId: string) => backfillWalletHashChain(walletId),
  });
}

export function useDeepCheckMutation() {
  return useMutation({
    mutationFn: (walletId: string) => runWalletDeepCheck(walletId),
  });
}
