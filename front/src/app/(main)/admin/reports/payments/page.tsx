'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function PaymentsReport() {
  return (
    <ReportShell
      title="Payment Report"
      description="Payment intent volume, success/failure rates, gateway performance, and transaction analysis"
      permission="payment.intent.view_admin"
    />
  );
}
