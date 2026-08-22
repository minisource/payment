import { describe, it, expect } from 'vitest';
import { ApiError } from '@/api/client';

describe('API Client', () => {
  it('ApiError parses standard backend error', () => {
    const err = new ApiError(
      403,
      'payment_forbidden',
      'Forbidden',
      'دسترسی ندارید',
      'permission',
      [],
      'req_123',
      'corr_456',
    );

    expect(err.status).toBe(403);
    expect(err.code).toBe('payment_forbidden');
    expect(err.userMessage).toBe('دسترسی ندارید');
    expect(err.category).toBe('permission');
    expect(err.requestId).toBe('req_123');
    expect(err.name).toBe('ApiError');
  });

  it('ApiError with no optional fields', () => {
    const err = new ApiError(500, 'internal_error', 'Something broke');
    expect(err.userMessage).toBeUndefined();
    expect(err.fieldErrors).toBeUndefined();
    expect(err.category).toBeUndefined();
  });
});
