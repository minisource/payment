'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores';
import { useTenant } from '@/hooks/use-tenant';
import { useLang } from '@/shared/i18n/LanguageProvider';
import { useTheme } from 'next-themes';
import { usePermission } from '@/hooks/use-permission';
import { AdminNotificationBell } from '@/features/notifications/components/AdminNotificationBell';
import {
  LayoutDashboard, Wallet, Receipt, ArrowLeftRight,
  Shield, AlertTriangle, Webhook, History, FileText,
  Settings, Activity, Server, Link2, Landmark,
  ChevronDown, Menu, X, UserCircle, LogOut, Users, RefreshCw,
  Languages, Sun, Moon, Bell,
} from 'lucide-react';

interface NavItem {
  label: string; href: string; icon: React.ElementType;
  permission?: string;
  children?: { label: string; href: string; icon: React.ElementType; permission?: string }[];
}

const navItems: NavItem[] = [
  { label: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { label: 'Wallets', href: '/admin/wallets', icon: Wallet, permission: 'payment.wallet.view_admin',
    children: [
      { label: 'All Wallets', href: '/admin/wallets', icon: Wallet, permission: 'payment.wallet.view_admin' },
      { label: 'Ledger Explorer', href: '/admin/ledger', icon: Receipt, permission: 'payment.wallet.view_admin' },
    ],
  },
  { label: 'Payments', href: '/admin/payments', icon: ArrowLeftRight, permission: 'payment.intent.view_admin',
    children: [
      { label: 'Payment Intents', href: '/admin/payments', icon: ArrowLeftRight, permission: 'payment.intent.view_admin' },
    ],
  },
  { label: 'Gateways', href: '/admin/gateways', icon: Server, permission: 'payment.gateway.config.view',
    children: [
      { label: 'Providers & Configs', href: '/admin/gateways', icon: Server, permission: 'payment.gateway.config.view' },
    ],
  },
  { label: 'Payment Links', href: '/admin/payment-links', icon: Link2, permission: 'payment.payment_link.view_admin' },
  { label: 'Payout & Withdrawals', href: '/admin/withdrawals', icon: Landmark, permission: 'payment.withdrawal.view_admin',
    children: [
      { label: 'Withdrawal Queue', href: '/admin/withdrawals', icon: Landmark, permission: 'payment.withdrawal.view_admin' },
      { label: 'Payout Accounts', href: '/admin/payout-accounts', icon: Users, permission: 'payment.withdrawal.view_admin' },
    ],
  },
  { label: 'Refunds', href: '/admin/refunds', icon: RefreshCw, permission: 'payment.refund.view_admin' },
  { label: 'Security', href: '/admin/security', icon: Shield, permission: 'payment.security.reports.view_admin',
    children: [
      { label: 'Risk Cases', href: '/admin/security', icon: AlertTriangle, permission: 'payment.risk.view' },
      { label: 'Reconciliation', href: '/admin/security/reconciliation', icon: Activity, permission: 'payment.reconciliation.view' },
    ],
  },
  { label: 'Approvals', href: '/admin/approvals', icon: Shield, permission: 'payment.admin_approval.view' },
  { label: 'Events', href: '/admin/events/outbox', icon: Webhook, permission: 'payment.outbox.view_admin',
    children: [
      { label: 'Outbox', href: '/admin/events/outbox', icon: Webhook, permission: 'payment.outbox.view_admin' },
      { label: 'Webhooks', href: '/admin/events/webhooks', icon: Webhook, permission: 'payment.webhook.view_admin' },
      { label: 'Routing', href: '/admin/events/routing', icon: Activity, permission: 'payment.webhook.view_admin' },
    ],
  },
  { label: 'Audit', href: '/admin/audit', icon: History, permission: 'payment.audit.view_admin' },
  { label: 'Reports', href: '/admin/reports', icon: FileText, permission: 'payment.security.reports.view_admin' },
  { label: 'Settings', href: '/admin/settings', icon: Settings, permission: 'payment.gateway.config.view',
    children: [
      { label: 'Payment Settings', href: '/admin/settings', icon: Settings },
      { label: 'Notification Settings', href: '/admin/notification-settings', icon: Bell },
      { label: 'System Health', href: '/admin/system-health', icon: Activity },
    ],
  },
];

export default function MainLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [expandedItems, setExpandedItems] = useState<string[]>(['Wallets', 'Payments', 'Events']);
  const { user, isAuthenticated, clearAuth } = useAuthStore();
  const { tenantId, tenantName, tenants, showTenantSwitcher, canViewAllTenants, setSelectedTenant, selectedTenant } = useTenant();
  const { can } = usePermission();
  const { lang, toggleLanguage } = useLang();
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  const toggleExpanded = (label: string) => {
    setExpandedItems(prev => prev.includes(label) ? prev.filter(i => i !== label) : [...prev, label]);
  };

  const isActive = (href: string) => href === '#' ? false : pathname === href || pathname.startsWith(href + '/');

  const handleLogout = () => {
    clearAuth();
    router.push('/login');
  };

  // Filter nav items by permission (only hide if permission is specified and user lacks it)
  const visibleItems = navItems.filter(item => {
    if (!item.permission) return true;
    return can(item.permission);
  });

  // For dev/testing: if no auth backend is available, assume dev-authenticated admin
  const effectiveAuth = isAuthenticated || process.env.NEXT_PUBLIC_DEV_MOCK_AUTH === 'true';
  const effectiveUser = user || (process.env.NEXT_PUBLIC_DEV_MOCK_AUTH === 'true' ? {
    id: 'dev-admin',
    email: 'admin@payment.local',
    firstName: 'Admin',
    lastName: 'User',
    roles: ['admin', 'super_admin'],
  } : null);

  const renderNavItem = (item: NavItem, mobile = false) => {
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedItems.includes(item.label);

    if (hasChildren && item.children) {
      const visibleChildren = item.children.filter(c => !c.permission || can(c.permission));
      if (visibleChildren.length === 0) return null;

      return (
        <div key={item.label}>
          <button onClick={() => toggleExpanded(item.label)}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors">
            <item.icon className="h-4 w-4" />
            <span className="flex-1 text-start">{item.label}</span>
            <ChevronDown className={cn('h-4 w-4 transition-transform', isExpanded && 'rotate-180')} />
          </button>
          {isExpanded && (
            <div className="ms-4 mt-1 space-y-1 border-s ps-3">
              {visibleChildren.map(child => (
                <Link key={child.href} href={child.href} onClick={() => mobile && setSidebarOpen(false)}
                  className={cn('flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                    isActive(child.href) ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground')}>
                  <child.icon className="h-4 w-4" />{child.label}
                </Link>
              ))}
            </div>
          )}
        </div>
      );
    }
    return (
      <Link key={item.href} href={item.href} onClick={() => mobile && setSidebarOpen(false)}
        className={cn('flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
          isActive(item.href) ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground')}>
        <item.icon className="h-4 w-4" />{item.label}
      </Link>
    );
  };

  return (
    <div className="flex min-h-screen">
      {/* Mobile overlay */}
      {sidebarOpen && <div className="fixed inset-0 z-40 bg-black/50 lg:hidden" onClick={() => setSidebarOpen(false)} />}

      {/* Sidebar */}
      <aside className={cn('fixed inset-y-0 z-50 flex w-64 flex-col bg-background transition-transform duration-300 lg:static lg:translate-x-0',
        lang === 'fa' ? 'right-0 border-l' : 'left-0 border-r',
        sidebarOpen ? 'translate-x-0' : (lang === 'fa' ? 'translate-x-full' : '-translate-x-full'))}>
        <div className="flex h-16 items-center justify-between border-b px-6">
          <Link href="/dashboard" className="text-lg font-bold text-primary">Payment Admin</Link>
          <Button variant="ghost" size="icon" className="lg:hidden" onClick={() => setSidebarOpen(false)}><X className="h-5 w-5" /></Button>
        </div>

        {/* Tenant selector */}
        {effectiveAuth && showTenantSwitcher && (
          <div className="border-b px-4 py-2">
            <select
              value={selectedTenant.scope === 'all' ? '__all__' : (tenantId ?? '')}
              onChange={(e) => {
                const val = e.target.value;
                if (val === '__all__') {
                  setSelectedTenant({ scope: 'all', tenantId: null, tenantName: 'All Tenants' });
                } else {
                  setSelectedTenant({ scope: 'single', tenantId: val });
                }
              }}
              className="w-full rounded-md border text-xs px-2 py-1.5 bg-background"
              title="Tenant selector"
            >
              {canViewAllTenants && <option value="__all__">All Tenants</option>}
              {tenants.map(t => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          </div>
        )}

        <nav className="flex-1 space-y-1 overflow-y-auto p-4">{visibleItems.map(item => renderNavItem(item))}</nav>

        <div className="border-t p-4">
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10"><UserCircle className="h-5 w-5 text-primary" /></div>
            <div className="flex-1 truncate">
              <p className="truncate text-sm font-medium">{effectiveUser?.firstName} {effectiveUser?.lastName}</p>
              <p className="truncate text-xs text-muted-foreground">{effectiveUser?.email}</p>
            </div>
            <Button variant="ghost" size="icon" onClick={handleLogout} title="Logout"><LogOut className="h-4 w-4" /></Button>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b bg-background/95 px-6 backdrop-blur">
          <Button variant="ghost" size="icon" className="lg:hidden" onClick={() => setSidebarOpen(true)}><Menu className="h-5 w-5" /></Button>
          <div className="flex-1" />
          {/* Notification bell */}
          <AdminNotificationBell />
          {/* Tenant badge */}
          {tenantName && (
            <span className="hidden rounded-full bg-blue-100 dark:bg-blue-900/30 px-3 py-1 text-xs font-medium text-blue-800 dark:text-blue-400 sm:inline-block">
              {tenantName}
            </span>
          )}
          <span className="hidden rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary sm:inline-block">
            {effectiveUser?.roles?.includes('super_admin') ? 'Super Admin' : 'Admin'}
          </span>
          {/* Theme toggle */}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {mounted && (theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />)}
          </Button>
          {/* Language toggle */}
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleLanguage}
            title={lang === 'fa' ? 'Switch to English' : 'تغییر به فارسی'}
            className="gap-1.5 text-xs font-medium"
          >
            <Languages className="h-4 w-4" />
            <span className="hidden sm:inline">{lang === 'fa' ? 'Fa' : 'En'}</span>
          </Button>
        </header>
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  );
}
