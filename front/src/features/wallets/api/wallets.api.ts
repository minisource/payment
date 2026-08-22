import { apiClient } from '@/api/client';
import type {
  WalletDto,
  WalletDetailDto,
  WalletTransactionDto,
  LedgerEntryDto,
  WalletAdjustmentRequest,
  WalletAdjustmentResponse,
  HashChainVerificationResult,
  WalletDeepCheckResult,
  ListWalletsParams,
  ListWalletTransactionsParams,
  ListWalletsResponse,
  ListWalletTransactionsResponse,
  ListWalletLedgerResponse,
} from '../types/wallet.types';

export async function listWallets(params?: ListWalletsParams, signal?: AbortSignal): Promise<ListWalletsResponse> {
  const { data } = await apiClient.get<ListWalletsResponse>('/api/v1/admin/wallets', { params, signal });
  return data;
}

export async function getWallet(walletId: string, signal?: AbortSignal): Promise<WalletDetailDto> {
  const { data } = await apiClient.get<WalletDetailDto>(`/api/v1/admin/wallets/${walletId}`, { signal });
  return data;
}

export async function listWalletTransactions(
  walletId: string,
  params?: ListWalletTransactionsParams,
  signal?: AbortSignal,
): Promise<ListWalletTransactionsResponse> {
  const { data } = await apiClient.get<ListWalletTransactionsResponse>(
    `/api/v1/admin/wallets/${walletId}/transactions`,
    { params, signal },
  );
  return data;
}

export async function listWalletLedger(
  walletId: string,
  params?: { skip?: number; take?: number },
  signal?: AbortSignal,
): Promise<ListWalletLedgerResponse> {
  const { data } = await apiClient.get<ListWalletLedgerResponse>(
    `/api/v1/admin/wallets/${walletId}/ledger`,
    { params, signal },
  );
  return data;
}

export async function creditWallet(walletId: string, request: WalletAdjustmentRequest): Promise<WalletAdjustmentResponse> {
  const { data } = await apiClient.post<WalletAdjustmentResponse>(
    `/api/v1/admin/wallets/${walletId}/adjustments/credit`,
    request,
  );
  return data;
}

export async function debitWallet(walletId: string, request: WalletAdjustmentRequest): Promise<WalletAdjustmentResponse> {
  const { data } = await apiClient.post<WalletAdjustmentResponse>(
    `/api/v1/admin/wallets/${walletId}/adjustments/debit`,
    request,
  );
  return data;
}

export async function verifyWalletHashChain(walletId: string): Promise<HashChainVerificationResult> {
  const { data } = await apiClient.post<HashChainVerificationResult>(
    `/api/v1/admin/diagnostics/ledger/${walletId}/verify-hash-chain`,
  );
  return data;
}

export async function backfillWalletHashChain(walletId: string): Promise<{ wallet_id: string; entries_fixed: number }> {
  const { data } = await apiClient.post<{ wallet_id: string; entries_fixed: number }>(
    `/api/v1/admin/diagnostics/ledger/${walletId}/backfill-hash-chain`,
  );
  return data;
}

export async function runWalletDeepCheck(walletId: string): Promise<WalletDeepCheckResult> {
  const { data } = await apiClient.post<WalletDeepCheckResult>(
    `/api/v1/admin/diagnostics/wallets/${walletId}/deep-check`,
  );
  return data;
}
