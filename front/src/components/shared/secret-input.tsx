'use client';

import { useState } from 'react';
import { Eye, EyeOff, Shield } from 'lucide-react';

interface SecretInputProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  /** Whether this is an existing secret (edit mode) — shows "configured" with placeholder */
  existing?: boolean;
  placeholder?: string;
  required?: boolean;
  className?: string;
}

export function SecretInput({ label, value, onChange, existing, placeholder, required, className }: SecretInputProps) {
  const [revealed, setRevealed] = useState(false);

  return (
    <div className={className}>
      <label className="flex items-center gap-1.5 text-xs font-medium mb-1">
        {label}
        {required && <span className="text-red-500">*</span>}
        {existing && (
          <span className="inline-flex items-center gap-0.5 rounded bg-green-100 dark:bg-green-900/30 px-1.5 py-0.5 text-[10px] font-normal text-green-700 dark:text-green-400">
            <Shield className="h-2.5 w-2.5" /> configured
          </span>
        )}
      </label>
      <div className="relative">
        <input
          type={revealed ? 'text' : 'password'}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={existing ? 'Leave blank to keep unchanged' : placeholder || 'Enter secret value…'}
          autoComplete="off"
          className="w-full rounded-md border bg-background px-3 py-2 pr-10 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/20"
        />
        <button
          type="button"
          onClick={() => setRevealed(!revealed)}
          className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
          title={revealed ? 'Hide' : 'Reveal'}
        >
          {revealed ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
        </button>
      </div>
      {existing && !value && (
        <p className="mt-1 text-[10px] text-muted-foreground">Blank = no change to existing secret</p>
      )}
      {!existing && value && (
        <p className="mt-1 text-[10px] text-amber-600 dark:text-amber-400">Secret will be encrypted. Save it securely — it cannot be viewed later.</p>
      )}
    </div>
  );
}
