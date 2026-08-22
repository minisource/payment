export interface ApprovalPolicyItemDto {
  id: string;
  name: string;
  description: string | null;
  tenant_id: string | null;
  application_code: string | null;
  operation_type: string;
  status: string;
  amount_threshold: string | null;
  currency: string | null;
  required_approvals: number;
  require_different_user: boolean;
  expires_after_minutes: number | null;
  created_at: string;
  updated_at: string;
}

export interface ApprovalPolicyCreateRequest {
  name: string;
  description?: string;
  tenant_id?: string | null;
  application_code?: string | null;
  operation_type: string;
  amount_threshold?: string;
  currency?: string;
  required_approvals: number;
  require_different_user?: boolean;
  expires_after_minutes?: number;
  conditions?: Record<string, unknown>;
}

export interface ApprovalPolicyUpdateRequest extends Partial<ApprovalPolicyCreateRequest> {
  status?: string;
}

export interface ApprovalPolicyActionResult {
  success: boolean;
  message: string;
}

export interface ListApprovalPoliciesParams {
  query?: string;
  skip?: number;
  take?: number;
  status?: string;
}

export type ListApprovalPoliciesResponse = {
  items: ApprovalPolicyItemDto[];
  total: number;
};
