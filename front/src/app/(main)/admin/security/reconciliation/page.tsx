'use client';

import { redirect } from 'next/navigation';

export default function ReconciliationPage() {
  redirect('/admin/security/reconciliation/batches');
}
