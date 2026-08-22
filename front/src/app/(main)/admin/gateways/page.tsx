'use client';

import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { RoutePermissionGuard } from '@/components/shared/permission-guards';
import { Server, Sliders, GitBranch, Shield } from 'lucide-react';

export default function GatewaysPage() {
  return (
    <RoutePermissionGuard permissions={['payment.gateway.config.view']}>
      <GatewaysContent />
    </RoutePermissionGuard>
  );
}

function GatewaysContent() {
  const links = [
    { label: 'Gateway Providers', href: '/admin/gateways/providers', icon: Server, desc: 'View provider catalog and capabilities', permission: 'payment.gateway.provider.view_admin' },
    { label: 'Gateway Configs', href: '/admin/gateways/configs', icon: Sliders, desc: 'Manage gateway configurations', permission: 'payment.gateway.config.view' },
    { label: 'Create Config', href: '/admin/gateways/configs/new', icon: Shield, desc: 'Add a new gateway configuration', permission: 'payment.gateway.config.create' },
    { label: 'Routing Policies', href: '/admin/gateways/routing-policies', icon: GitBranch, desc: 'Manage routing strategies and rules', permission: 'payment.gateway.routing.view_admin' },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Gateways</h1>
        <p className="text-sm text-muted-foreground">Payment gateway providers, configurations, and routing policies</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {links.map((link) => (
          <Link key={link.href} href={link.href}>
            <Card className="transition-shadow hover:shadow-md h-full">
              <CardHeader className="flex flex-row items-center gap-3 pb-2">
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                  <link.icon className="h-5 w-5 text-primary" />
                </div>
                <CardTitle className="text-sm">{link.label}</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-xs text-muted-foreground">{link.desc}</p>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
