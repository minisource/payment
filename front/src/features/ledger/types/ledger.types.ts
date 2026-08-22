import type { OffsetPage } from '@/shared/api/pagination';

export interface LedgerEntryDto {
  id: string;
  wallet_account_id: string;
  tenant_id: string;
  entry_type: string;
  direction: string;
  amount: string;
  currency: string;
  balance_available_before: string;
  balance_available_after: string;
  balance_locked_before: string;
  balance_locked_after: string;
  balance_pending_before: string;
  balance_pending_after: string;
  reason: string | null;
  reference_type: string | null;
  reference_id: string | null;
  previous_entry_hash: string;
  entry_hash: string;
  created_at: string;
}

export interface LedgerEntryDetailDto extends LedgerEntryDto {
  hash_algorithm?: string;
  hash_version?: number;
  metadata?: Record<string, unknown>;
}

export interface ListLedgerEntriesParams {
  query?: string;
  entryType?: string;
  direction?: string;
  currency?: string;
  dateFrom?: string;
  dateTo?: string;
  skip?: number;
  take?: number;
}

export type ListLedgerEntriesResponse = OffsetPage<LedgerEntryDto>;
