// ─── Refund Request ──────────────────────────────────────

export type RefundRequestStatus =
  | 'Requested'
  | 'PendingReview'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled'
  | 'HoldPending'
  | 'HoldCreated'
  | 'Processing'
  | 'GatewaySubmitted'
  | 'GatewaySucceeded'
  | 'GatewayFailed'
  | 'WalletDebitPending'
  | 'WalletDebited'
  | 'Completed'
  | 'Failed'
  | 'RequiresManualReview';

export interface RefundRequestDto {
  id: string;
  tenant_id: string;
  application_code: string | null;
  payment_intent_id: string | null;
  payment_transaction_id: string;
  wallet_id: string | null;
  requested_by_user_id: string;
  requested_by_admin_id: string | null;
  request_source: string;
  amount: number;
  currency: string;
  reason: string;
  admin_note: string | null;
  reject_reason: string | null;
  status: RefundRequestStatus;
  gateway_config_id: string;
  provider_code: string;
  gateway_name: string;
  gateway_refund_id: string | null;
  gateway_tracking_code: string | null;
  gateway_reference_id: string | null;
  wallet_hold_id: string | null;
  approved_by_user_id: string | null;
  approved_at: string | null;
  processed_by_user_id: string | null;
  processed_at: string | null;
  completed_at: string | null;
  failed_at: string | null;
  failure_code: string | null;
  failure_message: string | null;
  idempotency_key: string | null;
  correlation_id: string | null;
  request_id: string | null;
  created_at: string;
  updated_at: string;
  attempts: RefundGatewayAttemptDto[] | null;
}

export interface RefundGatewayAttemptDto {
  id: string;
  attempt_no: number;
  status: string;
  gateway_refund_id: string | null;
  gateway_tracking_code: string | null;
  gateway_status: string | null;
  http_status_code: number | null;
  error_code: string | null;
  error_message: string | null;
  started_at: string | null;
  finished_at: string | null;
}

// ─── Wallet Hold ─────────────────────────────────────────

export interface WalletHoldDto {
  id: string;
  tenant_id: string;
  wallet_account_id: string;
  amount: number;
  currency: string;
  status: string;
  reference_type: string;
  reference_id: string;
  reason: string;
  captured_at: string | null;
  released_at: string | null;
  expires_at: string | null;
  created_at: string;
}

// ─── Request DTOs ────────────────────────────────────────

export interface CreateRefundRequest {
  payment_transaction_id: string;
  amount: number;
  currency: string;
  reason: string;
  idempotency_key?: string;
}

export interface ApproveRefundRequest {
  admin_note?: string;
}

export interface RejectRefundRequest {
  reason: string;
}

// ─── List Params ─────────────────────────────────────────

export interface ListRefundsParams {
  status?: string;
  payment_transaction_id?: string;
  wallet_id?: string;
  provider_code?: string;
  currency?: string;
  from?: string;
  to?: string;
  query?: string;
  application_code?: string;
  tenant_id?: string;
  tenant_scope?: 'all' | 'single';
  skip?: number;
  take?: number;
}

export interface RefundListResponse {
  items: RefundRequestDto[];
  total: number;
  skip: number;
  take: number;
}
