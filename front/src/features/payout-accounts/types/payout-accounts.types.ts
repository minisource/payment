export interface PayoutAccountItemDto {
  id: string;
  tenant_id: string;
  owner_type: string;
  owner_id: string;
  account_type: 'card' | 'iban' | 'bank_account' | 'wallet' | 'other';
  holder_name: string;
  bank_name: string | null;
  card_number_masked: string | null;
  iban_masked: string | null;
  account_number_masked: string | null;
  currency: string;
  status: string;
  verified_at: string | null;
  rejected_at: string | null;
  rejection_reason: string | null;
  created_at: string;
  updated_at: string;
}

export interface ListPayoutAccountsParams {
  query?: string;
  skip?: number;
  take?: number;
  status?: string;
}

export type ListPayoutAccountsResponse = {
  items: PayoutAccountItemDto[];
  total: number;
};
