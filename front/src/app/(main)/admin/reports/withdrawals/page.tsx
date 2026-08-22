'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function WithdrawalsReport() {
  return (
    <ReportShell
      title="Withdrawal Report"
      description="Withdrawal volume, payout summaries, processing times, and success/failure rates"
      permission="payment.withdrawal.view_admin"
    />
  );
}
