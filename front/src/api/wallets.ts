import { get, post } from '@/api/client';
import type { WalletAccount, LedgerEntry, PaymentIntent } from '@/types/payment';

// ─── Extended types ─────────────────────────────────────────────

export interface WalletTransaction {
  id: string;
  walletAccountId: string;
  tenantId: string;
  type: string;
  status: string;
  amount: number;
  currency: string;
  referenceType: string | null;
  referenceId: string | null;
  idempotencyKey: string | null;
  postedAt: string | null;
  createdAt: string;
}

export interface WalletDetailResponse extends WalletAccount {
  transactions: WalletTransaction[];
  recentLedgerEntries: LedgerEntry[];
  relatedPaymentIntents: PaymentIntent[];
}

export interface WalletAdjustmentRequest {
  amount: string;
  currency: string;
  reason: string;
  metadata?: Record<string, unknown>;
}

export interface WalletAdjustmentResponse {
  id: string;
  walletId: string;
  type: string;
  amount: number;
  status: string;
  approvalRequestId?: string;
}

export interface HashChainVerificationResult {
  walletId: string;
  valid: boolean;
  invalidEntries: number;
  firstInvalidEntryId: string | null;
}

export interface WalletDeepCheckResult {
  walletId: string;
  consistent: boolean;
  issues: Array<{ type: string; description: string }>;
}

export interface WalletListParams {
  tenantId?: string;
  ownerType?: string;
  ownerId?: string;
  currency?: string;
  status?: string;
  query?: string;
  skip?: number;
  take?: number;
}

// ─── API calls ──────────────────────────────────────────────────

export async function listWallets(params?: WalletListParams) {
  return get<{ items: WalletAccount[]; total: number }>('/api/v1/admin/wallets', params as Record<string, unknown>);
}

export async function getWallet(walletId: string) {
  return get<WalletDetailResponse>(`/api/v1/admin/wallets/${walletId}`);
}

export async function listWalletTransactions(walletId: string, params?: { skip?: number; take?: number; type?: string; status?: string }) {
  return get<{ items: WalletTransaction[]; total: number }>(`/api/v1/admin/wallets/${walletId}/transactions`, params as Record<string, unknown>);
}

export async function listWalletLedger(walletId: string, params?: { skip?: number; take?: number }) {
  return get<{ items: LedgerEntry[]; total: number }>(`/api/v1/admin/wallets/${walletId}/ledger`, params as Record<string, unknown>);
}

export async function adminCreditWallet(walletId: string, request: WalletAdjustmentRequest) {
  return post<WalletAdjustmentResponse>(`/api/v1/admin/wallets/${walletId}/adjustments/credit`, request);
}

export async function adminDebitWallet(walletId: string, request: WalletAdjustmentRequest) {
  return post<WalletAdjustmentResponse>(`/api/v1/admin/wallets/${walletId}/adjustments/debit`, request);
}

export async function verifyWalletHashChain(walletId: string) {
  return post<HashChainVerificationResult>(`/api/v1/admin/diagnostics/ledger/${walletId}/verify-hash-chain`);
}

export async function backfillWalletHashChain(walletId: string) {
  return post<{ walletId: string; entriesFixed: number }>(`/api/v1/admin/diagnostics/ledger/${walletId}/backfill-hash-chain`);
}

export async function runWalletDeepCheck(walletId: string) {
  return post<WalletDeepCheckResult>(`/api/v1/admin/diagnostics/wallets/${walletId}/deep-check`);
}
