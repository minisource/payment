export interface ApprovalRequestItemDto {
  id: string;
  operation_type: string;
  tenant_id: string;
  application_code: string | null;
  status: string;
  requested_by_user_id: string;
  subject_type: string;
  subject_id: string;
  required_approvals: number;
  approvals_count: number;
  expires_at: string | null;
  created_at: string;
  updated_at: string;
}

export interface ApprovalDecision {
  id: string;
  user_id: string;
  decision: string;
  note: string | null;
  decided_at: string;
}

export interface ApprovalActionResult {
  success: boolean;
  message: string;
  execution_result?: Record<string, unknown>;
}

export interface ListApprovalRequestsParams {
  query?: string;
  skip?: number;
  take?: number;
  status?: string;
}

export type ListApprovalRequestsResponse = {
  items: ApprovalRequestItemDto[];
  total: number;
};
