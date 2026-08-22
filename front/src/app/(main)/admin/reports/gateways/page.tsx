'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function GatewaysReport() {
  return (
    <ReportShell
      title="Gateway Report"
      description="Gateway provider performance, success rates, routing analysis, and configuration audit"
      permission="payment.gateway.config.view"
    />
  );
}
