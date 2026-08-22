import type { OffsetPage } from '@/shared/api/pagination';

export type PaymentIntentStatus = 'pending' | 'started' | 'requires_action' | 'succeeded' | 'failed' | 'cancelled' | 'expired';

export interface PaymentIntentDto {
  id: string;
  tenant_id: string;
  amount: string;
  currency: string;
  status: PaymentIntentStatus;
  provider_code: string | null;
  description: string | null;
  external_reference_type: string | null;
  external_reference_id: string | null;
  wallet_behavior: string;
  return_url: string | null;
  expires_at: string | null;
  created_at: string;
  updated_at: string;
}

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

export interface TimelineEventDto {
  status: string;
  timestamp: string;
  label: string;
  detail?: string;
}

export interface PaymentIntentDetailDto extends PaymentIntentDto {
  fee_amount: string;
  net_amount: string;
  payer_wallet_id: string | null;
  recipient_wallet_id: string | null;
  payment_link_id: string | null;
  succeeded_at: string | null;
  failed_at: string | null;
  gateway_transactions: PaymentTransactionDto[];
  timeline: TimelineEventDto[];
  metadata?: Record<string, unknown>;
}

export interface ListPaymentIntentsParams {
  query?: string;
  status?: PaymentIntentStatus;
  providerCode?: string;
  currency?: string;
  dateFrom?: string;
  dateTo?: string;
  skip?: number;
  take?: number;
}

export type ListPaymentIntentsResponse = OffsetPage<PaymentIntentDto>;
