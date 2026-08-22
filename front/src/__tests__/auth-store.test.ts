import { describe, it, expect, beforeEach } from 'vitest';
import { useAuthStore } from '@/stores/auth.store';

// Reset store before each test
beforeEach(() => {
  useAuthStore.setState({
    user: null,
    tokens: null,
    isAuthenticated: false,
  });
});

describe('Auth Store', () => {
  it('starts unauthenticated', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
    expect(state.tokens).toBeNull();
  });

  it('sets auth state on login', () => {
    const user = { id: 'u1', email: 'test@test.com', firstName: 'Test', lastName: 'User', roles: ['admin'], permissions: ['payment.wallet.view_admin'] };
    const tokens = { accessToken: 'at-123', refreshToken: 'rt-456' };

    useAuthStore.getState().setAuth(user, tokens);

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.user?.email).toBe('test@test.com');
    expect(state.tokens?.accessToken).toBe('at-123');
  });

  it('clears auth state on logout', () => {
    const user = { id: 'u1', email: 'test@test.com', firstName: 'Test', lastName: 'User', roles: ['admin'] };
    const tokens = { accessToken: 'at-123', refreshToken: 'rt-456' };
    useAuthStore.getState().setAuth(user, tokens);
    useAuthStore.getState().clearAuth();

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });

  it('hasRole checks roles correctly', () => {
    const user = { id: 'u1', email: 'test@test.com', firstName: 'Test', lastName: 'User', roles: ['admin'] };
    useAuthStore.getState().setAuth(user, { accessToken: 't', refreshToken: 'r' });

    expect(useAuthStore.getState().hasRole('admin')).toBe(true);
    expect(useAuthStore.getState().hasRole('user')).toBe(false);
  });

  it('hasRole returns true for super_admin', () => {
    const user = { id: 'u1', email: 'sa@test.com', firstName: 'SA', lastName: 'User', roles: ['super_admin'] };
    useAuthStore.getState().setAuth(user, { accessToken: 't', refreshToken: 'r' });

    expect(useAuthStore.getState().hasRole('admin')).toBe(true);
  });

  it('hasPermission checks permission list', () => {
    const user = { id: 'u1', email: 'test@test.com', firstName: 'Test', lastName: 'User', roles: ['admin'], permissions: ['payment.wallet.view_admin'] };
    useAuthStore.getState().setAuth(user, { accessToken: 't', refreshToken: 'r' });

    expect(useAuthStore.getState().hasPermission('payment.wallet.view_admin')).toBe(true);
    expect(useAuthStore.getState().hasPermission('payment.wallet.adjust_admin')).toBe(false);
  });

  it('super_admin has all permissions', () => {
    const user = { id: 'u1', email: 'sa@test.com', firstName: 'SA', lastName: 'User', roles: ['super_admin'] };
    useAuthStore.getState().setAuth(user, { accessToken: 't', refreshToken: 'r' });

    expect(useAuthStore.getState().hasPermission('any.permission')).toBe(true);
  });
});
