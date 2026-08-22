'use client';

import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { Loading } from '@/components/shared/states';
import { getEventsOverview } from '@/api/events';
import { Mail, Globe, GitBranch, Search, Activity, Server, AlertTriangle, Clock } from 'lucide-react';

export default function EventsPage() {
  return (
    <RoutePermissionGuard permissions={['payment.events.view_admin']}>
      <EventsContent />
    </RoutePermissionGuard>
  );
}

function EventsContent() {
  const { data, isLoading } = useQuery({
    queryKey: ['events-overview'],
    queryFn: getEventsOverview,
  });

  if (isLoading) return <Loading />;

  const metrics = [
    { label: 'Pending Outbox', value: data?.pendingOutboxEvents ?? '—', icon: Clock, color: 'text-blue-600' },
    { label: 'Processing', value: data?.processingOutboxEvents ?? '—', icon: Activity, color: 'text-yellow-600' },
    { label: 'Failed Outbox', value: data?.failedOutboxEvents ?? '—', icon: AlertTriangle, color: data?.failedOutboxEvents ? 'text-red-600' : 'text-muted-foreground' },
    { label: 'Dead-Lettered', value: data?.deadLetteredEvents ?? '—', icon: AlertTriangle, color: data?.deadLetteredEvents ? 'text-red-600' : 'text-muted-foreground' },
    { label: 'Active Webhooks', value: data?.activeWebhookSubscriptions ?? '—', icon: Globe, color: 'text-green-600' },
    { label: 'Failed Deliveries', value: data?.failedWebhookDeliveries ?? '—', icon: AlertTriangle, color: data?.failedWebhookDeliveries ? 'text-red-600' : 'text-muted-foreground' },
  ];

  const sections = [
    { href: '/admin/events/outbox', label: 'Outbox Events', desc: 'Monitor and manage event delivery pipeline', icon: Mail },
    { href: '/admin/events/webhooks', label: 'Webhooks', desc: 'Subscriptions, secrets, and delivery logs', icon: Globe },
    { href: '/admin/events/webhook-deliveries', label: 'Webhook Deliveries', desc: 'Delivery attempts, responses, and retries', icon: GitBranch },
    { href: '/admin/events/routing', label: 'Event Routing', desc: 'Routing rules and target configuration', icon: Activity },
    { href: '/admin/audit', label: 'Audit Logs', desc: 'Tamper-evident audit trail with hash-chain', icon: Search },
    { href: '/admin/system-health', label: 'System Health', desc: 'API, database, dispatcher, and dependency status', icon: Server },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Events & Operations</h1>
        <p className="text-sm text-muted-foreground">Outbox, webhooks, event routing, audit, and system health</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {metrics.map((m) => {
          const Icon = m.icon;
          return (
            <Card key={m.label}>
              <CardContent className="p-4 flex items-center gap-3">
                <Icon className={`h-5 w-5 ${m.color}`} />
                <div>
                  <p className="text-xs text-muted-foreground">{m.label}</p>
                  <p className="text-xl font-bold tabular-nums">{m.value}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Dispatcher status if available */}
      {data?.dispatcherStatus && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Dispatcher</CardTitle></CardHeader>
          <CardContent><span className="capitalize text-sm">{data.dispatcherStatus}</span></CardContent>
        </Card>
      )}

      <h2 className="text-lg font-semibold">Quick Links</h2>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((s) => {
          const Icon = s.icon;
          return (
            <Link key={s.href} href={s.href}>
              <Card className="h-full transition-colors hover:border-primary/50">
                <CardContent className="p-4 flex items-start gap-3">
                  <Icon className="mt-0.5 h-5 w-5 text-muted-foreground" />
                  <div>
                    <h3 className="font-semibold">{s.label}</h3>
                    <p className="mt-1 text-xs text-muted-foreground">{s.desc}</p>
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
