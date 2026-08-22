import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { PermissionGuard } from '@/components/shared/permission-guards';
import { PermissionProvider } from '@/hooks/use-permission';
import { useAuthStore } from '@/stores/auth.store';

const wrapper = ({ children }: { children: ReactNode }) => (
  <PermissionProvider>{children}</PermissionProvider>
);

describe('PermissionGuard', () => {
  it('hides content when user lacks permission', () => {
    // User without permissions
    useAuthStore.setState({
      user: { id: 'u1', email: 't@t.com', firstName: 'T', lastName: 'U', roles: ['user'], permissions: [] },
      tokens: { accessToken: 't', refreshToken: 'r' },
      isAuthenticated: true,
    });

    render(
      <PermissionGuard permission="payment.wallet.view_admin" fallback={<span>Access Denied</span>}>
        <span>Secret Content</span>
      </PermissionGuard>,
      { wrapper },
    );

    expect(screen.getByText('Access Denied')).toBeInTheDocument();
    expect(screen.queryByText('Secret Content')).not.toBeInTheDocument();
  });

  it('shows content when user has required permission', () => {
    useAuthStore.setState({
      user: { id: 'u1', email: 'a@a.com', firstName: 'A', lastName: 'U', roles: ['admin'], permissions: ['payment.wallet.view_admin'] },
      tokens: { accessToken: 't', refreshToken: 'r' },
      isAuthenticated: true,
    });

    render(
      <PermissionGuard permission="payment.wallet.view_admin" fallback={<span>Access Denied</span>}>
        <span>Secret Content</span>
      </PermissionGuard>,
      { wrapper },
    );

    expect(screen.getByText('Secret Content')).toBeInTheDocument();
    expect(screen.queryByText('Access Denied')).not.toBeInTheDocument();
  });

  it('anyOf mode passes if user has at least one permission', () => {
    useAuthStore.setState({
      user: { id: 'u1', email: 'a@a.com', firstName: 'A', lastName: 'U', roles: ['admin'], permissions: ['payment.risk.view'] },
      tokens: { accessToken: 't', refreshToken: 'r' },
      isAuthenticated: true,
    });

    render(
      <PermissionGuard permission={['payment.wallet.view_admin', 'payment.risk.view']} mode="anyOf" fallback={<span>Denied</span>}>
        <span>Allowed</span>
      </PermissionGuard>,
      { wrapper },
    );

    expect(screen.getByText('Allowed')).toBeInTheDocument();
  });
});
