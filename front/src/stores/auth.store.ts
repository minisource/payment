import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

/** Minimal user info matching auth/front model */
export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions?: string[];
  tenantId?: string;
}

interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

interface AuthState {
  user: UserInfo | null;
  tokens: AuthTokens | null;
  isAuthenticated: boolean;

  setAuth: (user: UserInfo, tokens: AuthTokens) => void;
  clearAuth: () => void;
  updateUser: (user: Partial<UserInfo>) => void;
  updateTokens: (tokens: AuthTokens) => void;
  hasRole: (role: string) => boolean;
  hasPermission: (permission: string) => boolean;
  isAdmin: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      tokens: null,
      isAuthenticated: false,

      setAuth: (user, tokens) => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('accessToken', tokens.accessToken);
          localStorage.setItem('refreshToken', tokens.refreshToken);
        }
        set({ user, tokens, isAuthenticated: true });
      },

      clearAuth: () => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
        set({ user: null, tokens: null, isAuthenticated: false });
      },

      updateUser: (updates) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...updates } : null,
        })),

      updateTokens: (tokens) => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('accessToken', tokens.accessToken);
          localStorage.setItem('refreshToken', tokens.refreshToken);
        }
        set({ tokens });
      },

      hasRole: (role: string) => {
        const { user } = get();
        if (!user) return false;
        return user.roles.includes(role) || user.roles.includes('super_admin');
      },

      hasPermission: (permission: string) => {
        const { user } = get();
        if (!user) return false;
        if (user.roles.includes('super_admin')) return true;
        if (!user.permissions) return false;
        return user.permissions.includes(permission);
      },

      isAdmin: () => {
        const { user } = get();
        if (!user) return false;
        return user.roles.includes('admin') || user.roles.includes('super_admin');
      },
    }),
    {
      name: 'payment-auth-storage',
      storage: createJSONStorage(() => {
        if (typeof window === 'undefined') return null as unknown as Storage;
        return window.localStorage;
      }),
      partialize: (state) => ({
        user: state.user,
        tokens: state.tokens,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
