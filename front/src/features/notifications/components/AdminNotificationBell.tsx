'use client';

import { useState, useRef, useEffect } from 'react';
import Link from 'next/link';
import { Bell, CheckCheck, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useUnreadCountQuery, useNotificationsQuery } from '@/features/notifications/api/notifications.queries';
import { useMarkAllAsReadMutation } from '@/features/notifications/api/notifications.mutations';
import { useTenant } from '@/hooks/use-tenant';
import { cn } from '@/lib/utils';
import { formatDistanceToNow } from 'date-fns';

export function AdminNotificationBell() {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const { tenantId, selectedTenant } = useTenant();

  const { data: unreadData } = useUnreadCountQuery();
  const { data: listData, isLoading } = useNotificationsQuery(
    {
      page: 1,
      pageSize: 5,
      tenantScope: selectedTenant.scope,
      tenantId: selectedTenant.scope === 'single' ? tenantId ?? undefined : undefined,
    },
    open,
  );
  const markAllMutation = useMarkAllAsReadMutation();

  // Close on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    if (open) document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [open]);

  const unreadCount = unreadData?.unreadCount ?? 0;
  const notifications = listData?.data ?? [];

  return (
    <div ref={ref} className="relative">
      <Button
        variant="ghost"
        size="icon"
        onClick={() => setOpen(!open)}
        className="relative"
        title="Notifications"
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-0.5 text-[10px] font-bold text-white">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </Button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 w-80 rounded-md border bg-popover shadow-lg">
          <div className="flex items-center justify-between border-b px-4 py-3">
            <span className="text-sm font-semibold">Notifications</span>
            {unreadCount > 0 && (
              <Button
                variant="ghost"
                size="sm"
                className="h-auto px-2 py-0.5 text-xs"
                onClick={(e) => {
                  e.stopPropagation();
                  markAllMutation.mutate();
                }}
                disabled={markAllMutation.isPending}
              >
                <CheckCheck className="mr-1 h-3 w-3" />
                Mark all read
              </Button>
            )}
          </div>
          <div className="max-h-80 overflow-y-auto">
            {isLoading ? (
              <div className="flex items-center justify-center py-6">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              </div>
            ) : notifications.length === 0 ? (
              <div className="py-6 text-center text-sm text-muted-foreground">
                No notifications yet
              </div>
            ) : (
              notifications.map((n) => (
                <Link
                  key={n.id}
                  href="/admin/notifications"
                  onClick={() => setOpen(false)}
                  className="flex flex-col items-start gap-1 border-b px-4 py-3 transition-colors hover:bg-accent/50 last:border-b-0"
                >
                  <div className="flex w-full items-start justify-between gap-2">
                    <span className={cn('text-sm font-medium', !n.readAt && 'text-foreground')}>
                      {n.subject || 'Notification'}
                    </span>
                    {!n.readAt && (
                      <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-blue-500" />
                    )}
                  </div>
                  <p className="line-clamp-2 text-xs text-muted-foreground">
                    {n.body.length > 100 ? n.body.slice(0, 100) + '...' : n.body}
                  </p>
                  <span className="text-[10px] text-muted-foreground">
                    {formatDistanceToNow(new Date(n.createdAt), { addSuffix: true })}
                  </span>
                </Link>
              ))
            )}
          </div>
          <Link
            href="/admin/notifications"
            onClick={() => setOpen(false)}
            className="block border-t px-4 py-2.5 text-center text-sm font-medium text-primary hover:bg-accent/50"
          >
            View all notifications
          </Link>
        </div>
      )}
    </div>
  );
}
