import type { OffsetPage } from '@/shared/api/pagination';

export interface PaymentTransactionDto {
  id: string;
  tenant_id: string;
  payment_intent_id: string;
  gateway_config_id: string;
  provider_code: string;
  status: string;
  amount: string;
  currency: string;
  authority: string | null;
  gateway_reference_id: string | null;
  trace_number: string | null;
  card_pan_masked: string | null;
  verified_at: string | null;
  created_at: string;
}

export interface PaymentTransactionDetailDto extends PaymentTransactionDto {
  payment_intent: {
    id: string;
    amount: string;
    currency: string;
    status: string;
    description: string | null;
  };
  callback_payload?: Record<string, unknown>;
  verify_payload?: Record<string, unknown>;
  wallet_posting_result?: {
    wallet_id: string;
    ledger_entry_id: string;
    amount: string;
    type: string;
  };
  timeline: Array<{
    status: string;
    timestamp: string;
    label: string;
  }>;
}

export interface ListPaymentTransactionsParams {
  query?: string;
  status?: string;
  providerCode?: string;
  currency?: string;
  dateFrom?: string;
  dateTo?: string;
  skip?: number;
  take?: number;
}

export type ListPaymentTransactionsResponse = OffsetPage<PaymentTransactionDto>;
