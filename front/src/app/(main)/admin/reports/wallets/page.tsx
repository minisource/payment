'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function WalletsReport() {
  return (
    <ReportShell
      title="Wallet Report"
      description="Wallet balances, activity trends, liability by currency, and consistency status"
      permission="payment.wallet.view_admin"
    />
  );
}
