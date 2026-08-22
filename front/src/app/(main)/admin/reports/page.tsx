'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { PermissionGuard } from '@/components/shared/permission-guards';
import Link from 'next/link';
import {
  BarChart3, Wallet, ArrowLeftRight, Server, Landmark,
  Shield, Webhook, FileText,
} from 'lucide-react';

interface ReportCategory {
  label: string;
  description: string;
  href: string;
  icon: React.ElementType;
  permission: string;
}

const reportCategories: ReportCategory[] = [
  { label: 'Financial Overview', description: 'Wallet liability, total assets, currency breakdown', href: '/admin/reports/financial-overview', icon: BarChart3, permission: 'payment.security.reports.view_admin' },
  { label: 'Wallets', description: 'Wallet balances, activity, and liability reports', href: '/admin/reports/wallets', icon: Wallet, permission: 'payment.wallet.view_admin' },
  { label: 'Payments', description: 'Payment intents, success rates, gateway performance', href: '/admin/reports/payments', icon: ArrowLeftRight, permission: 'payment.intent.view_admin' },
  { label: 'Gateways', description: 'Gateway provider performance and routing analysis', href: '/admin/reports/gateways', icon: Server, permission: 'payment.gateway.config.view' },
  { label: 'Withdrawals', description: 'Withdrawal volume, payout summaries, processing times', href: '/admin/reports/withdrawals', icon: Landmark, permission: 'payment.withdrawal.view_admin' },
  { label: 'Security', description: 'Risk cases, reconciliation, ledger integrity', href: '/admin/reports/security', icon: Shield, permission: 'payment.security.reports.view_admin' },
  { label: 'Events', description: 'Outbox delivery, webhook performance, event routing', href: '/admin/reports/events', icon: Webhook, permission: 'payment.outbox.view_admin' },
];

export default function ReportsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Reports</h1>
        <p className="text-sm text-muted-foreground">Financial reports, analytics, and operational summaries</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {reportCategories.map((cat) => (
          <PermissionGuard key={cat.href} permission={cat.permission}>
            <Link href={cat.href}>
              <Card className="transition-shadow hover:shadow-md h-full">
                <CardHeader className="flex flex-row items-center gap-3 pb-2">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                    <cat.icon className="h-5 w-5 text-primary" />
                  </div>
                  <CardTitle className="text-base">{cat.label}</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">{cat.description}</p>
                </CardContent>
              </Card>
            </Link>
          </PermissionGuard>
        ))}
      </div>

      {/* Info card */}
      <Card className="border-dashed">
        <CardContent className="p-4">
          <div className="flex items-center gap-3">
            <FileText className="h-5 w-5 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              Detailed report views with filters, charts, and export will be implemented in later phases.
              Each report category above links to a shell page with filter layout placeholders.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
