import type { OffsetPage } from '@/shared/api/pagination';

export interface WithdrawalItem {
  id: string;
  tenant_id: string;
  owner_type: string;
  owner_id: string;
  wallet_id: string;
  payout_account_id: string;
  account_type: string;
  card_masked: string | null;
  iban_masked: string | null;
  amount: string;
  fee_amount: string;
  net_amount: string;
  currency: string;
  status: string;
  created_at: string;
  updated_at: string;
}

export interface ListWithdrawalsParams {
  query?: string;
  status?: string;
  skip?: number;
  take?: number;
}

export type ListWithdrawalsResponse = OffsetPage<WithdrawalItem>;
