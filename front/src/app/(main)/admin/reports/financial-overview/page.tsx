'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function FinancialOverviewReport() {
  return (
    <ReportShell
      title="Financial Overview"
      description="Wallet liability, total assets, currency breakdown, and balance trends"
      permission="payment.security.reports.view_admin"
    />
  );
}
