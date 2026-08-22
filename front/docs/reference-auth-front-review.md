# Reference: auth/front Inspection Report

This document captures patterns and structures from `MiniSource/auth/front`
that influenced the Payment Admin Frontend design.

## Detected Stack

| Aspect | Value |
|--------|-------|
| **Framework** | Next.js 15 (App Router) |
| **Language** | TypeScript 5.7 |
| **Package Manager** | npm |
| **Styling** | Tailwind CSS 3.4 + shadcn/ui |
| **State (Server)** | @tanstack/react-query |
| **State (Client)** | Zustand 5 (persisted) |
| **HTTP Client** | Axios |
| **Icons** | Lucide React |
| **Forms** | React Hook Form + Zod |
| **Testing** | Vitest + @testing-library/react + jsdom |
| **Linting** | ESLint 9 (flat config) |
| **Formatting** | Prettier + prettier-plugin-tailwindcss |

## Scripts

| Script | Purpose |
|--------|---------|
| `dev` / `dev:turbo` | Next.js dev server |
| `build` | Production build |
| `lint` / `lint:fix` | ESLint |
| `format` / `format:check` | Prettier |
| `type-check` | tsc --noEmit |
| `test` / `test:coverage` | Vitest |
| `docker:build/dev/prod` | Container build |

## Patterns Reused in Payment Frontend

### 1. Zustand Auth Store (`stores/auth.store.ts`)
- Persistent auth state with `create()(persist(...))`
- `setAuth(user, tokens)`, `clearAuth()`, `updateUser()`, `updateTokens()`
- `hasRole()`, `hasPermission()`, `isAdmin()` convenience methods
- JSON serialization via `createJSONStorage(() => localStorage)`

### 2. Providers Hierarchy (`components/providers/index.tsx`)
- `QueryClientProvider` wrapping `ThemeProvider`
- Nested context providers for auth, tenant, permission
- Sonner `<Toaster>` for toast notifications

### 3. API Client Pattern
- Axios instance with base URL from env
- Request interceptor: inject `Authorization: Bearer`, `X-Tenant-Id`, `X-Application-Code`
- Response interceptor: parse errors into typed `ApiError`
- Helper functions: `get<T>`, `post<T>`, `patch<T>`, `del<T>`

### 4. App Router Structure
- `src/app/(main)/` — authenticated routes group
- Layout with sidebar navigation, permission filtering
- `layout.tsx` at each level for shared UI

### 5. Path Aliases (`tsconfig.json`)
```json
{
  "@/*": ["./src/*"],
  "@components/*": ["./src/components/*"],
  "@ui/*": ["./src/components/ui/*"],
  "@lib/*": ["./src/lib/*"],
  "@hooks/*": ["./src/hooks/*"],
  "@stores/*": ["./src/stores/*"],
  "@api/*": ["./src/api/*"],
  "@types/*": ["./src/types/*"]
}
```

### 6. ESLint Config
- Flat config using `@eslint/eslintrc` compat
- Extends `next/core-web-vitals` + `next/typescript`
- Custom rules for unused vars, explicit any, no-console

### 7. Vitest Config
- `jsdom` environment, `globals: true`
- Setup file for `@testing-library/jest-dom`
- Path alias resolution matching tsconfig

## Patterns Intentionally NOT Reused

| Pattern | Reason |
|---------|--------|
| Full auth hooks (login/register/OTP/reset) | Payment frontend assumes auth is handled by auth service; uses token-based auth only |
| Admin user/role/permission management hooks | Not in scope — payment admin manages payment entities, not auth entities |
| OAuth/Google integration | Not required for payment admin |
| Session management | Auth service responsibility — payment frontend just uses tokens |

## Deviations from auth/front

| Area | auth/front | payment/front |
|------|-----------|---------------|
| Auth state source | Full login flow with tokens | Dev mock OR real token from auth service |
| Permission model | Role-based + permission strings | Permission strings only (loaded from user.permissions[]) |
| Tenant handling | Admin can manage tenants | Tenant selector for filtering payment data |

---

> Last updated: 2026-07-07
