'use client';

import React, { useState, useCallback } from 'react';
import { useTenant } from '@/hooks/use-tenant';
import { useNotificationsQuery } from '@/features/notifications/api/notifications.queries';
import { useMarkAsReadMutation, useMarkAllAsReadMutation } from '@/features/notifications/api/notifications.mutations';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Loading, EmptyState } from '@/components/shared/states';
import { SearchInput } from '@/components/shared/filters';
import { cn } from '@/lib/utils';
import {
  Bell, BellRing, Mail, CheckCheck, Eye,
  ExternalLink, AlertTriangle, RefreshCw, Landmark, Shield, ChevronLeft, ChevronRight,
} from 'lucide-react';
import { formatDistanceToNow, format } from 'date-fns';
import type { NotificationDto } from '../types/notification.types';

const CHANNEL_ICONS: Record<string, React.ElementType> = {
  in_app: Bell, sms: BellRing, email: Mail, push: BellRing, webhook: ExternalLink,
};

const KEY_ICONS: Record<string, React.ElementType> = {
  refund: RefreshCw, withdrawal: Landmark, payment: Bell, gateway: Shield,
  reconciliation: AlertTriangle, risk: AlertTriangle,
};

function getNotificationIcon(n: NotificationDto) {
  const iconKey = Object.keys(KEY_ICONS).find((k) =>
    n.subject?.toLowerCase().includes(k) || n.body?.toLowerCase().includes(k),
  );
  const Icon = iconKey ? KEY_ICONS[iconKey] : (CHANNEL_ICONS[n.type] ?? Bell);
  return React.createElement(Icon, { className: 'h-4 w-4' });
}

export function AdminNotificationsPage() {
  const { tenantId, selectedTenant, setSelectedTenant, canViewAllTenants, tenants } = useTenant();

  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [search, setSearch] = useState('');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data, isLoading } = useNotificationsQuery(
    {
      page,
      pageSize,
      status: statusFilter !== 'all' ? statusFilter : undefined,
      search: search || undefined,
      tenantScope: selectedTenant.scope,
      tenantId: selectedTenant.scope === 'single' ? tenantId ?? undefined : undefined,
    },
    true,
  );

  const markReadMutation = useMarkAsReadMutation();
  const markAllMutation = useMarkAllAsReadMutation();

  const handleMarkRead = useCallback(
    (id: string) => markReadMutation.mutate(id),
    [markReadMutation],
  );

  const notifications = data?.data ?? [];
  const selectedNotification = selectedId ? notifications.find((n) => n.id === selectedId) : null;
  const totalPages = data?.totalPages ?? 0;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Notifications</h1>
          <p className="text-sm text-muted-foreground">Admin notification center for payment events</p>
        </div>
        <div className="flex items-center gap-2">
          {selectedTenant.scope === 'all' && canViewAllTenants && tenants.length > 1 && (
            <select
              value="__all__"
              onChange={(e) => {
                const v = e.target.value;
                if (v !== '__all__') setSelectedTenant({ scope: 'single', tenantId: v });
              }}
              className="h-9 rounded-md border bg-background px-3 text-sm"
            >
              <option value="__all__">All Tenants</option>
              {tenants.map((t) => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          )}
          <Button
            variant="outline"
            size="sm"
            onClick={() => markAllMutation.mutate()}
            disabled={markAllMutation.isPending}
          >
            <CheckCheck className="mr-2 h-4 w-4" />
            Mark all read
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="h-9 rounded-md border bg-background px-3 text-sm"
        >
          <option value="all">All statuses</option>
          <option value="pending">Pending</option>
          <option value="sent">Sent</option>
          <option value="failed">Failed</option>
          <option value="delivered">Delivered</option>
        </select>
        <SearchInput value={search} onChange={setSearch} placeholder="Search notifications..." className="w-64" />
      </div>

      {/* Content */}
      <div className="grid gap-4 lg:grid-cols-3">
        {/* List */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium">
              {data ? `${data.total} notifications` : 'Notifications'}
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            {isLoading ? (
              <Loading />
            ) : notifications.length === 0 ? (
              <EmptyState icon={<Bell className="h-10 w-10" />} title="No notifications" description="Payment notifications will appear here" />
            ) : (
              <div className="divide-y">
                {notifications.map((n) => (
                  <button
                    key={n.id}
                    onClick={() => setSelectedId(n.id)}
                    className={cn(
                      'flex w-full items-start gap-3 px-4 py-3 text-left transition-colors hover:bg-accent/50',
                      selectedId === n.id && 'bg-accent',
                      !n.readAt && 'bg-blue-50/50 dark:bg-blue-950/20',
                    )}
                  >
                    <div className={cn('mt-0.5 shrink-0', !n.readAt ? 'text-blue-500' : 'text-muted-foreground')}>
                      {getNotificationIcon(n)}
                    </div>
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <span className={cn('text-sm font-medium', !n.readAt && 'font-semibold')}>
                          {n.subject || 'Notification'}
                        </span>
                        {!n.readAt && <span className="h-2 w-2 rounded-full bg-blue-500" />}
                      </div>
                      <p className="line-clamp-1 text-xs text-muted-foreground">
                        {n.body.length > 120 ? n.body.slice(0, 120) + '...' : n.body}
                      </p>
                      <span className="text-[10px] text-muted-foreground">
                        {formatDistanceToNow(new Date(n.createdAt), { addSuffix: true })}
                      </span>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </CardContent>
          {totalPages > 1 && (
            <div className="flex items-center justify-between border-t px-4 py-2">
              <span className="text-xs text-muted-foreground">
                Page {page} of {totalPages} ({data?.total ?? 0} total)
              </span>
              <div className="flex items-center gap-1">
                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}
        </Card>

        {/* Detail */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium">Details</CardTitle>
          </CardHeader>
          <CardContent>
            {selectedNotification ? (
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <Badge variant={selectedNotification.readAt ? 'outline' : 'default'}>
                    {selectedNotification.readAt ? 'Read' : 'Unread'}
                  </Badge>
                  <span className="text-xs text-muted-foreground">
                    {format(new Date(selectedNotification.createdAt), 'MMM d, yyyy HH:mm')}
                  </span>
                </div>
                <div>
                  <h3 className="font-semibold">{selectedNotification.subject || 'No subject'}</h3>
                  <p className="mt-1 text-sm text-muted-foreground whitespace-pre-wrap">
                    {selectedNotification.body}
                  </p>
                </div>
                <div className="space-y-1 text-xs text-muted-foreground">
                  <div className="flex justify-between">
                    <span>Channel</span>
                    <Badge variant="secondary" className="text-[10px]">{selectedNotification.type}</Badge>
                  </div>
                  <div className="flex justify-between"><span>Status</span><span>{selectedNotification.status}</span></div>
                  {selectedNotification.templateKey && (
                    <div className="flex justify-between">
                      <span>Template</span><span className="font-mono">{selectedNotification.templateKey}</span>
                    </div>
                  )}
                  {selectedNotification.errorMessage && (
                    <div className="mt-2 rounded bg-red-50 p-2 text-red-800 dark:bg-red-950/30 dark:text-red-300">
                      {selectedNotification.errorMessage}
                    </div>
                  )}
                </div>
                {!selectedNotification.readAt && (
                  <Button variant="outline" size="sm" className="w-full"
                    onClick={() => handleMarkRead(selectedNotification.id)}
                    disabled={markReadMutation.isPending}>
                    <Eye className="mr-2 h-4 w-4" />Mark as read
                  </Button>
                )}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-8 text-center">
                <Bell className="mb-2 h-8 w-8 text-muted-foreground/40" />
                <p className="text-sm text-muted-foreground">Select a notification to view details</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
