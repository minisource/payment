import { get, post } from '@/api/client';

// ═══════════════════════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════════════════════

export interface RiskEvaluation {
  id: string;
  operationType: string;
  tenantId: string;
  applicationCode: string | null;
  subjectType: string;
  subjectId: string;
  riskLevel: string;
  riskScore: number;
  decision: string;
  triggeredRulesCount: number;
  createdAt: string;
}

export interface RiskEvaluationDetail extends RiskEvaluation {
  triggeredRules: string[];
  velocityData?: Record<string, unknown>;
  inputContext?: Record<string, unknown>;
  outputReason: string | null;
  metadata?: Record<string, unknown>;
}

export interface RiskCase {
  id: string;
  caseType: string;
  tenantId: string;
  applicationCode: string | null;
  subjectType: string;
  subjectId: string;
  severity: string;
  status: string;
  title: string;
  assignedToUserId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface RiskCaseDetail extends RiskCase {
  description: string | null;
  relatedEvaluationIds: string[];
  triggeredRules: string[];
  timeline: RiskCaseTimelineEvent[];
  notes: RiskCaseNote[];
  resolutionNote: string | null;
  resolvedBy: string | null;
  metadata?: Record<string, unknown>;
}

export interface RiskCaseTimelineEvent {
  status: string;
  timestamp: string;
  label: string;
  userId?: string;
}

export interface RiskCaseNote {
  id: string;
  content: string;
  userId: string;
  createdAt: string;
}

export interface RiskCaseActionResult {
  success: boolean;
  message: string;
}

// ═══════════════════════════════════════════════════════════════
// Evaluations
// ═══════════════════════════════════════════════════════════════

export async function listRiskEvaluations(params?: Record<string, unknown>) {
  return get<{ items: RiskEvaluation[]; total: number }>('/api/v1/admin/reports/financial-security/risk-cases', params);
}

export async function getRiskEvaluation(evaluationId: string) {
  return get<RiskEvaluationDetail>(`/api/v1/admin/reports/financial-security/risk-cases/${evaluationId}`);
}

// ═══════════════════════════════════════════════════════════════
// Cases
// ═══════════════════════════════════════════════════════════════

export async function listRiskCases(params?: Record<string, unknown>) {
  return get<{ items: RiskCase[]; total: number }>('/api/v1/admin/reports/financial-security/risk-cases', params);
}

export async function getRiskCase(caseId: string) {
  return get<RiskCaseDetail>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}`);
}

export async function acknowledgeRiskCase(caseId: string) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/acknowledge`);
}

export async function assignRiskCase(caseId: string, req: { assignedToUserId: string }) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/assign`, req);
}

export async function escalateRiskCase(caseId: string) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/escalate`);
}

export async function resolveRiskCase(caseId: string, req: { reason: string }) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/resolve`, req);
}

export async function markRiskCaseFalsePositive(caseId: string, req: { reason: string }) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/false-positive`, req);
}

export async function closeRiskCase(caseId: string, req: { reason: string }) {
  return post<RiskCaseActionResult>(`/api/v1/admin/reports/financial-security/risk-cases/${caseId}/close`, req);
}
