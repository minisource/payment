import type { PublicConfig } from '@/types/public-config';
import { PAYMENT_API_URL } from '@/config/constants';

/**
 * Fetches the public configuration from the Payment backend.
 * This call does NOT require authentication.
 */
export async function fetchPublicConfig(): Promise<PublicConfig> {
  const baseURL = PAYMENT_API_URL;

  const response = await fetch(`${baseURL}/v1/system/public-config`, {
    method: 'GET',
    headers: { 'Accept': 'application/json' },
    cache: 'no-cache',
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch public config: ${response.status}`);
  }

  return response.json();
}
