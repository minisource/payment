import { get } from '@/api/client';
import type { LedgerEntry } from '@/types/payment';

export interface LedgerListParams {
  tenantId?: string;
  walletId?: string;
  entryType?: string;
  direction?: string;
  currency?: string;
  amountMin?: number;
  amountMax?: number;
  referenceType?: string;
  referenceId?: string;
  dateFrom?: string;
  dateTo?: string;
  query?: string;
  skip?: number;
  take?: number;
}

export interface LedgerEntryDetail extends LedgerEntry {
  previousEntryHash: string;
  entryHash: string;
  hashAlgorithm?: string;
  hashVersion?: number;
  metadata?: Record<string, unknown>;
}

export async function listLedgerEntries(params?: LedgerListParams) {
  return get<{ items: LedgerEntry[]; total: number }>('/api/v1/admin/ledger', params as Record<string, unknown>);
}

export async function getLedgerEntry(ledgerEntryId: string) {
  return get<LedgerEntryDetail>(`/api/v1/admin/ledger/${ledgerEntryId}`);
}
