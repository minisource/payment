'use client';

import { useState } from 'react';
import { cn } from '@/lib/utils';
import { ChevronDown, ChevronRight, Eye, EyeOff, Copy } from 'lucide-react';

interface SafePayloadViewerProps {
  data?: Record<string, unknown> | null;
  title?: string;
  className?: string;
}

/** Safely displays JSON payloads with automatic redaction of sensitive fields */
export function SafePayloadViewer({ data, title = 'Payload', className }: SafePayloadViewerProps) {
  const [expanded, setExpanded] = useState(false);
  const [showRedacted, setShowRedacted] = useState(false);

  if (!data) {
    return (
      <div className={cn('rounded-md border p-3', className)}>
        <p className="text-xs text-muted-foreground">No data available</p>
      </div>
    );
  }

  const SENSITIVE_KEYS = ['secret', 'token', 'password', 'api_key', 'apiKey', 'authorization', 'card_number', 'cardNumber', 'iban', 'bank_account', 'cvv', 'pan'];
  const SENSITIVE_PREFIXES = ['card', 'iban', 'account_number', 'private_'];

  function isSensitive(key: string): boolean {
    const lower = key.toLowerCase();
    if (SENSITIVE_KEYS.some(k => lower.includes(k))) return true;
    if (SENSITIVE_PREFIXES.some(p => lower.startsWith(p))) return true;
    return false;
  }

  function redactValue(key: string, value: unknown): unknown {
    if (!showRedacted && isSensitive(key)) {
      if (typeof value === 'string') {
        if (value.length <= 6) return '••••';
        return value.slice(0, 2) + '••••' + value.slice(-2);
      }
      return '••••';
    }
    return value;
  }

  function redactObject(obj: Record<string, unknown>): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(obj)) {
      if (value && typeof value === 'object' && !Array.isArray(value)) {
        result[key] = redactObject(value as Record<string, unknown>);
      } else {
        result[key] = redactValue(key, value);
      }
    }
    return result;
  }

  const safeData = redactObject(data);
  const jsonString = JSON.stringify(safeData, null, 2);

  return (
    <div className={cn('rounded-md border', className)}>
      <div className="flex items-center justify-between border-b bg-muted/30 px-3 py-2">
        <button onClick={() => setExpanded(!expanded)} className="flex items-center gap-1 text-xs font-medium hover:text-foreground">
          {expanded ? <ChevronDown className="h-3.5 w-3.5" /> : <ChevronRight className="h-3.5 w-3.5" />}
          {title}
        </button>
        <div className="flex items-center gap-2">
          <button onClick={() => setShowRedacted(!showRedacted)} className="text-xs text-muted-foreground hover:text-foreground" title={showRedacted ? 'Hide sensitive values' : 'Show sensitive values'}>
            {showRedacted ? <EyeOff className="h-3.5 w-3.5" /> : <Eye className="h-3.5 w-3.5" />}
          </button>
          <button onClick={() => navigator.clipboard.writeText(JSON.stringify(data, null, 2))} className="text-xs text-muted-foreground hover:text-foreground" title="Copy raw payload">
            <Copy className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>
      {expanded && (
        <pre className="max-h-[400px] overflow-auto p-3 text-xs font-mono whitespace-pre-wrap break-all">
          {jsonString}
        </pre>
      )}
    </div>
  );
}
