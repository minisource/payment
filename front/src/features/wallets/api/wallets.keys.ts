import type { ListWalletsParams, ListWalletTransactionsParams } from '../types/wallet.types';

export const walletKeys = {
  all: ['wallets'] as const,
  lists: () => [...walletKeys.all, 'list'] as const,
  list: (params: ListWalletsParams) => [...walletKeys.lists(), params] as const,
  details: () => [...walletKeys.all, 'detail'] as const,
  detail: (walletId: string) => [...walletKeys.details(), walletId] as const,
  transactions: (walletId: string, params?: ListWalletTransactionsParams) =>
    [...walletKeys.all, 'transactions', walletId, params] as const,
  ledger: (walletId: string, params?: { skip?: number; take?: number }) =>
    [...walletKeys.all, 'ledger', walletId, params] as const,
  diagnostics: (walletId: string) => [...walletKeys.all, 'diagnostics', walletId] as const,
};
