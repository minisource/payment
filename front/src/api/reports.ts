import { get } from '@/api/client';

export interface ReportCategory {
  id: string;
  title: string;
  description: string;
  permission: string;
  href: string;
}

export interface ReportsOverviewResponse {
  categories: ReportCategory[];
}

/** Fetch reports overview — uses dashboard overview as canonical source */
export async function getReportsOverview(): Promise<ReportsOverviewResponse> {
  return get<ReportsOverviewResponse>('/api/v1/admin/dashboard/overview');
}

// Individual report fetch functions (documented for future phases)

export async function getFinancialReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/financial-overview', params);
}

export async function getWalletReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/wallets', params);
}

export async function getPaymentReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/payments', params);
}

export async function getGatewayReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/gateways', params);
}

export async function getWithdrawalReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/withdrawals', params);
}

export async function getSecurityReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/security', params);
}

export async function getEventsReport(params?: Record<string, unknown>) {
  return get('/api/v1/admin/reports/events', params);
}
