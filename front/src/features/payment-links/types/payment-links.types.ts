export interface PaymentLinkItemDto {
  id: string;
  tenant_id: string;
  application_code: string | null;
  owner_type: string;
  owner_id: string;
  title: string;
  description: string | null;
  amount_type: 'fixed' | 'free' | 'minimum';
  amount: string | null;
  currency: string;
  status: string;
  max_uses: number | null;
  used_count: number;
  total_paid: string;
  safe_public_url: string | null;
  expires_at: string | null;
  created_at: string;
  updated_at: string;
}

export interface PaymentLinkDetailDto extends PaymentLinkItemDto {
  successful_payments: Array<{ id: string; amount: string; currency: string; created_at: string }>;
  related_payment_intents: Array<{ id: string; amount: string; currency: string; status: string }>;
  metadata?: Record<string, unknown>;
}

export interface ListPaymentLinksParams {
  query?: string;
  skip?: number;
  take?: number;
  status?: string;
}

export type ListPaymentLinksResponse = {
  items: PaymentLinkItemDto[];
  total: number;
};
