import type { OffsetPage } from '@/shared/api/pagination';

// ─── Wallet Account ─────────────────────────────────────

export type WalletStatus = 'active' | 'frozen' | 'disabled' | 'deleted';

export interface WalletDto {
  id: string;
  tenant_id: string;
  application_code: string;
  owner_type: string;
  owner_id: string;
  available_balance: string;
  locked_balance: string;
  pending_balance: string;
  currency: string;
  status: WalletStatus;
  created_at: string;
  updated_at: string;
}

// ─── Wallet Transaction ─────────────────────────────────

export type WalletTransactionType = 'credit' | 'debit' | 'fee' | 'hold' | 'release' | 'refund';

export interface WalletTransactionDto {
  id: string;
  wallet_account_id: string;
  tenant_id: string;
  type: WalletTransactionType;
  status: string;
  amount: string;
  currency: string;
  reference_type: string | null;
  reference_id: string | null;
  idempotency_key: string | null;
  posted_at: string | null;
  created_at: string;
}

// ─── Wallet Detail ──────────────────────────────────────

export interface WalletDetailDto extends WalletDto {
  transactions: WalletTransactionDto[];
  recent_ledger_entries: LedgerEntryDto[];
  related_payment_intents: PaymentIntentRefDto[];
}

export interface LedgerEntryDto {
  id: string;
  wallet_id: string;
  amount: string;
  currency: string;
  entry_type: string;
  reference_type: string | null;
  reference_id: string | null;
  description: string | null;
  balance_before: string;
  balance_after: string;
  created_at: string;
}

export interface PaymentIntentRefDto {
  id: string;
  amount: string;
  currency: string;
  status: string;
  created_at: string;
}

// ─── Wallet Adjustment ──────────────────────────────────

export interface WalletAdjustmentRequest {
  amount: string;
  currency: string;
  reason: string;
  metadata?: Record<string, unknown>;
}

export interface WalletAdjustmentResponse {
  id: string;
  wallet_id: string;
  type: string;
  amount: string;
  status: string;
  approval_request_id?: string;
}

// ─── Wallet Diagnostics ─────────────────────────────────

export interface HashChainVerificationResult {
  wallet_id: string;
  valid: boolean;
  invalid_entries: number;
  first_invalid_entry_id: string | null;
}

export interface WalletDeepCheckResult {
  wallet_id: string;
  consistent: boolean;
  issues: Array<{ type: string; description: string }>;
}

// ─── List Params ────────────────────────────────────────

export interface ListWalletsParams {
  tenant_id?: string;
  owner_type?: string;
  owner_id?: string;
  currency?: string;
  status?: WalletStatus;
  query?: string;
  skip?: number;
  take?: number;
}

export interface ListWalletTransactionsParams {
  skip?: number;
  take?: number;
  type?: WalletTransactionType;
  status?: string;
}

// ─── API Response Types ─────────────────────────────────

export type ListWalletsResponse = OffsetPage<WalletDto>;
export type ListWalletTransactionsResponse = OffsetPage<WalletTransactionDto>;
export type ListWalletLedgerResponse = OffsetPage<LedgerEntryDto>;
