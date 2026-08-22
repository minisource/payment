'use client';

import { useQuery } from '@tanstack/react-query';
import { ledgerKeys } from './ledger.keys';
import { listLedgerEntries, getLedgerEntry } from './ledger.api';
import type { ListLedgerEntriesParams } from '../types/ledger.types';

export function useLedgerEntriesQuery(params: ListLedgerEntriesParams) {
  return useQuery({
    queryKey: ledgerKeys.list(params),
    queryFn: ({ signal }) => listLedgerEntries(params, signal),
    staleTime: 15_000,
    placeholderData: (prev) => prev,
  });
}

export function useLedgerEntryDetailQuery(ledgerEntryId: string | undefined) {
  return useQuery({
    queryKey: ledgerKeys.detail(ledgerEntryId!),
    queryFn: ({ signal }) => getLedgerEntry(ledgerEntryId!, signal),
    enabled: !!ledgerEntryId,
  });
}
