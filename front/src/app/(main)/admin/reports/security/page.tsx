'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function SecurityReport() {
  return (
    <ReportShell
      title="Security Report"
      description="Risk cases, reconciliation summaries, ledger integrity, and wallet consistency"
      permission="payment.security.reports.view_admin"
    />
  );
}
