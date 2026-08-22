'use client';

import { ThemeProvider } from 'next-themes';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { useState } from 'react';
import { PermissionProvider } from '@/hooks/use-permission';
import { TenantProvider, ApplicationProvider } from '@/hooks/use-tenant';
import { LanguageProvider } from '@/shared/i18n/LanguageProvider';

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
  }));

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
        <ApplicationProvider>
          <LanguageProvider>
          <TenantProvider>
            <PermissionProvider>
              {children}
              <Toaster position="top-right" richColors closeButton />
            </PermissionProvider>
          </TenantProvider>
          </LanguageProvider>
        </ApplicationProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
