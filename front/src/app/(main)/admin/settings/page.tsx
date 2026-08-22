'use client';

import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Settings, Activity, Bell } from 'lucide-react';

export default function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Payment service and tenant configuration</p>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <Link href="/admin/gateways">
          <Card className="h-full transition-colors hover:bg-accent/50">
            <CardHeader className="pb-2"><Settings className="h-5 w-5 text-primary" /><CardTitle className="text-sm">Gateway Settings</CardTitle></CardHeader>
            <CardContent><p className="text-xs text-muted-foreground">Gateway configs, routing policies, and providers</p></CardContent>
          </Card>
        </Link>
        <Link href="/admin/notification-settings">
          <Card className="h-full transition-colors hover:bg-accent/50">
            <CardHeader className="pb-2"><Bell className="h-5 w-5 text-primary" /><CardTitle className="text-sm">Notification Settings</CardTitle></CardHeader>
            <CardContent><p className="text-xs text-muted-foreground">Manage notification preferences for admin and users</p></CardContent>
          </Card>
        </Link>
        <Link href="/admin/system-health">
          <Card className="h-full transition-colors hover:bg-accent/50">
            <CardHeader className="pb-2"><Activity className="h-5 w-5 text-primary" /><CardTitle className="text-sm">System Health</CardTitle></CardHeader>
            <CardContent><p className="text-xs text-muted-foreground">Service health, dependencies, and dispatcher status</p></CardContent>
          </Card>
        </Link>
      </div>
    </div>
  );
}
