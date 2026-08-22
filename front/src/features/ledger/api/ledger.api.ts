import { apiClient } from '@/api/client';
import type { ListLedgerEntriesParams, ListLedgerEntriesResponse, LedgerEntryDetailDto } from '../types/ledger.types';

export async function listLedgerEntries(params?: ListLedgerEntriesParams, signal?: AbortSignal): Promise<ListLedgerEntriesResponse> {
  const { data } = await apiClient.get<ListLedgerEntriesResponse>('/api/v1/admin/ledger', { params, signal });
  return data;
}

export async function getLedgerEntry(ledgerEntryId: string, signal?: AbortSignal): Promise<LedgerEntryDetailDto> {
  const { data } = await apiClient.get<LedgerEntryDetailDto>(`/api/v1/admin/ledger/${ledgerEntryId}`, { signal });
  return data;
}
