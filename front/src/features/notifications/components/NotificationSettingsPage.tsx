'use client';

import { useTenant } from '@/hooks/use-tenant';
import { useNotificationSettingsQuery } from '@/features/notifications/api/notifications.queries';
import { useUpdateNotificationSettingMutation } from '@/features/notifications/api/notifications.mutations';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Loading } from '@/components/shared/states';
import { Users, Shield, ToggleLeft, ToggleRight } from 'lucide-react';

export function NotificationSettingsPage() {
  const { tenantId, selectedTenant } = useTenant();
  const { data, isLoading } = useNotificationSettingsQuery();
  const updateMutation = useUpdateNotificationSettingMutation();

  const settings = data?.items ?? [];

  // Group by recipient type
  const userSettings = settings.filter((s) => s.recipient_type === 'user');
  const adminSettings = settings.filter((s) => s.recipient_type === 'admin');

  const handleToggle = (key: string, currentlyEnabled: boolean) => {
    updateMutation.mutate({ notificationKey: key, isEnabled: !currentlyEnabled });
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Notification Settings</h1>
        <p className="text-sm text-muted-foreground">
          Enable or disable payment notification types per tenant.
          {selectedTenant.scope === 'single' && tenantId && (
            <span className="ml-1 font-medium">
              Tenant: {selectedTenant.tenantName || tenantId.slice(0, 8)}
            </span>
          )}
        </p>
      </div>

      {isLoading ? (
        <Loading />
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          {/* User Notifications */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm font-medium">
                <Users className="h-4 w-4" />
                User Notifications
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {userSettings.length === 0 ? (
                <p className="text-sm text-muted-foreground">No user notification settings found.</p>
              ) : (
                userSettings.map((setting) => (
                  <div key={setting.notification_key} className="flex items-center justify-between rounded-lg border p-3">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium">{setting.notification_key}</p>
                      <p className="text-xs text-muted-foreground">{setting.description}</p>
                    </div>
                    <Button
                      variant={setting.is_enabled ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => handleToggle(setting.notification_key, setting.is_enabled)}
                      disabled={updateMutation.isPending}
                      className="h-8 gap-1 px-3 text-xs"
                    >
                      {setting.is_enabled ? (
                        <ToggleRight className="h-4 w-4" />
                      ) : (
                        <ToggleLeft className="h-4 w-4" />
                      )}
                      {setting.is_enabled ? 'On' : 'Off'}
                    </Button>
                  </div>
                ))
              )}
            </CardContent>
          </Card>

          {/* Admin Notifications */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm font-medium">
                <Shield className="h-4 w-4" />
                Admin Notifications
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {adminSettings.length === 0 ? (
                <p className="text-sm text-muted-foreground">No admin notification settings found.</p>
              ) : (
                adminSettings.map((setting) => (
                  <div key={setting.notification_key} className="flex items-center justify-between rounded-lg border p-3">
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <p className="text-sm font-medium">{setting.notification_key}</p>
                        <Badge variant="outline" className="text-[10px]">{setting.recipient_type}</Badge>
                      </div>
                      <p className="text-xs text-muted-foreground">{setting.description}</p>
                    </div>
                    <Button
                      variant={setting.is_enabled ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => handleToggle(setting.notification_key, setting.is_enabled)}
                      disabled={updateMutation.isPending}
                      className="h-8 gap-1 px-3 text-xs"
                    >
                      {setting.is_enabled ? (
                        <ToggleRight className="h-4 w-4" />
                      ) : (
                        <ToggleLeft className="h-4 w-4" />
                      )}
                      {setting.is_enabled ? 'On' : 'Off'}
                    </Button>
                  </div>
                ))
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
