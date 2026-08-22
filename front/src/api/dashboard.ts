import { get } from '@/api/client';

// ─── Dashboard types ───────────────────────────────────────────

export interface LiabilitySummary {
  currency: string;
  availableTotal: number;
  lockedTotal: number;
  pendingTotal: number;
}

export interface PaymentMetricSummary {
  successfulToday: number;
  failedToday: number;
  successRate: number;
  totalVolume: number;
  currency: string;
}

export interface WithdrawalMetricSummary {
  pendingCount: number;
  pendingAmount: number;
  approvedCount: number;
  paidAmount: number;
  currency: string;
}

export interface GatewayMetricSummary {
  successRate: number;
  failedTransactions: number;
  topProvider: string | null;
}

export interface SecurityMetricSummary {
  openRiskCases: number;
  criticalRiskCases: number;
  unresolvedRecItems: number;
  walletConsistencyIssues: number;
  pendingApprovals: number;
}

export interface EventDeliveryMetricSummary {
  pendingOutbox: number;
  failedOutbox: number;
  deadLetteredOutbox: number;
  failedWebhookDeliveries: number;
}

export interface ChartDataPoint {
  label: string;
  value: number;
  date?: string;
}

export interface LatestPaymentIntentItem {
  id: string;
  tenantId: string;
  amount: number;
  currency: string;
  status: string;
  providerCode: string | null;
  createdAt: string;
}

export interface LatestWithdrawalItem {
  id: string;
  tenantId: string;
  amount: number;
  currency: string;
  status: string;
  netAmount: number;
  createdAt: string;
}

export interface LatestRiskCaseItem {
  id: string;
  tenantId: string;
  severity: string;
  status: string;
  description: string;
  openedAt: string;
}

export interface LatestReconciliationItem {
  id: string;
  tenantId: string;
  reconciliationType: string;
  status: string;
  amount: number;
  currency: string;
  createdAt: string;
}

export interface LatestEventDeliveryItem {
  id: string;
  eventType: string;
  status: string;
  lastErrorMessage: string | null;
  createdAt: string;
}

export interface DashboardOverviewResponse {
  liability: LiabilitySummary[];
  payments: PaymentMetricSummary[];
  withdrawals: WithdrawalMetricSummary[];
  gateways: GatewayMetricSummary;
  security: SecurityMetricSummary;
  events: EventDeliveryMetricSummary;
}

export interface DashboardParams {
  dateFrom?: string;
  dateTo?: string;
}

// ─── API calls ──────────────────────────────────────────────────

/** Fetch aggregated dashboard overview */
export async function getDashboardOverview(params?: DashboardParams): Promise<DashboardOverviewResponse> {
  return get<DashboardOverviewResponse>('/api/v1/admin/dashboard/overview', params as Record<string, unknown>);
}

/** Fetch payment volume chart data */
export async function getPaymentVolumeChart(params?: DashboardParams): Promise<ChartDataPoint[]> {
  return get<ChartDataPoint[]>('/api/v1/admin/dashboard/charts/payment-volume', params as Record<string, unknown>);
}

/** Fetch withdrawal volume chart data */
export async function getWithdrawalVolumeChart(params?: DashboardParams): Promise<ChartDataPoint[]> {
  return get<ChartDataPoint[]>('/api/v1/admin/dashboard/charts/withdrawal-volume', params as Record<string, unknown>);
}

/** Fetch gateway success/failure by provider chart data */
export async function getGatewayChart(params?: DashboardParams): Promise<ChartDataPoint[]> {
  return get<ChartDataPoint[]>('/api/v1/admin/dashboard/charts/gateway-status', params as Record<string, unknown>);
}

/** Fetch risk cases by severity chart data */
export async function getRiskChart(params?: DashboardParams): Promise<ChartDataPoint[]> {
  return get<ChartDataPoint[]>('/api/v1/admin/dashboard/charts/risk-severity', params as Record<string, unknown>);
}

/** Fetch event delivery status distribution chart data */
export async function getEventDeliveryChart(params?: DashboardParams): Promise<ChartDataPoint[]> {
  return get<ChartDataPoint[]>('/api/v1/admin/dashboard/charts/event-delivery', params as Record<string, unknown>);
}

/** Fetch latest payment intents (compact) */
export async function getLatestPaymentIntents(params?: DashboardParams & { limit?: number }): Promise<{ items: LatestPaymentIntentItem[] }> {
  return get<{ items: LatestPaymentIntentItem[] }>('/api/v1/admin/dashboard/latest-payment-intents', params as Record<string, unknown>);
}

/** Fetch latest withdrawals (compact) */
export async function getLatestWithdrawals(params?: DashboardParams & { limit?: number }): Promise<{ items: LatestWithdrawalItem[] }> {
  return get<{ items: LatestWithdrawalItem[] }>('/api/v1/admin/dashboard/latest-withdrawals', params as Record<string, unknown>);
}

/** Fetch latest risk cases (compact) */
export async function getLatestRiskCases(params?: DashboardParams & { limit?: number }): Promise<{ items: LatestRiskCaseItem[] }> {
  return get<{ items: LatestRiskCaseItem[] }>('/api/v1/admin/dashboard/latest-risk-cases', params as Record<string, unknown>);
}

/** Fetch latest reconciliation items (compact) */
export async function getLatestReconciliationItems(params?: DashboardParams & { limit?: number }): Promise<{ items: LatestReconciliationItem[] }> {
  return get<{ items: LatestReconciliationItem[] }>('/api/v1/admin/dashboard/latest-reconciliation-items', params as Record<string, unknown>);
}

/** Fetch latest failed event deliveries */
export async function getLatestEventDeliveries(params?: DashboardParams & { limit?: number }): Promise<{ items: LatestEventDeliveryItem[] }> {
  return get<{ items: LatestEventDeliveryItem[] }>('/api/v1/admin/dashboard/latest-event-deliveries', params as Record<string, unknown>);
}
