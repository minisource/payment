'use client';

import { ReportShell } from '@/components/shared/report-shell';

export default function EventsReport() {
  return (
    <ReportShell
      title="Event Report"
      description="Outbox delivery, webhook performance, event routing analysis, and delivery latency"
      permission="payment.outbox.view_admin"
    />
  );
}
