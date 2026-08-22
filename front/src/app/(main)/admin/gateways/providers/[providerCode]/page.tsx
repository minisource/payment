'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorState, DetailPageSkeleton } from '@/components/shared/states';
import { t } from '@/shared/i18n/translations';
import { GatewayHealthBadge, ProviderBadge } from '@/components/shared/gateway-badges';
import { DataTable, DataTableColumn } from '@/components/shared/data-table';
import { getGatewayProvider, type ConfigSchemaField } from '@/api/gateways';
import { Shield, Key } from 'lucide-react';

export default function GatewayProviderDetailPage() {
  const { providerCode } = useParams<{ providerCode: string }>();

  const { data: provider, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['gateway-provider', providerCode],
    queryFn: () => getGatewayProvider(providerCode),
  });

  if (isLoading) return <DetailPageSkeleton cards={3} />;
  if (isError || !provider) return <ErrorState error={(error as Error)?.message || t('notFound.gatewayProvider')} onRetry={() => refetch()} />;

  const schemaColumns: DataTableColumn<ConfigSchemaField>[] = [
    { key: 'key', header: 'Key', render: (f) => <span className="font-mono text-xs">{f.key}</span> },
    { key: 'label', header: 'Label', render: (f) => <span className="text-xs">{f.label}</span> },
    { key: 'type', header: 'Type', render: (f) => <span className="font-mono text-xs">{f.type}</span> },
    { key: 'required', header: 'Required', render: (f) => f.required ? <span className="text-xs text-red-600 font-medium">Yes</span> : <span className="text-xs text-muted-foreground">No</span> },
    { key: 'sensitive', header: 'Sensitive', render: (f) => f.sensitive ? <span className="inline-flex items-center gap-1 text-xs text-amber-600 font-medium"><Key className="h-3 w-3" /> Yes</span> : <span className="text-xs">—</span> },
    { key: 'description', header: 'Description', render: (f) => <span className="text-xs text-muted-foreground">{f.description}</span> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/gateways/providers" className="text-sm text-muted-foreground hover:text-foreground">← Gateway Providers</Link>
        <div className="mt-1 flex items-center gap-3">
          <h1 className="text-xl font-bold">{provider.displayName}</h1>
          <ProviderBadge provider={provider.code} />
          <GatewayHealthBadge health={provider.enabled ? 'healthy' : 'unknown'} />
        </div>
        <p className="mt-1 text-sm text-muted-foreground">{provider.description}</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Adapter Type</span><span className="font-mono text-xs">{provider.adapterType}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Enabled</span><span className={provider.enabled ? 'text-green-600 font-medium' : 'text-red-600 font-medium'}>{provider.enabled ? 'Yes' : 'No'}</span></div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Supported Currencies</CardTitle></CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-1">
              {provider.supportedCurrencies?.map((c) => (
                <span key={c} className="rounded bg-muted px-2 py-1 text-xs font-mono">{c}</span>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm">Supported Operations</CardTitle></CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
            {provider.supportedOperations?.map((op) => (
              <span key={op} className="rounded-full bg-blue-50 dark:bg-blue-900/20 px-3 py-1 text-xs font-medium text-blue-700 dark:text-blue-400">{op}</span>
            ))}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle className="text-sm flex items-center gap-2"><Shield className="h-4 w-4" /> Configuration Schema</CardTitle></CardHeader>
        <CardContent className="p-0">
          {provider.configSchema?.length ? (
            <DataTable columns={schemaColumns} data={provider.configSchema} keyExtractor={(f) => f.key} />
          ) : (
            <p className="p-4 text-sm text-muted-foreground">No configuration schema available</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
