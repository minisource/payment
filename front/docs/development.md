# Development Guide

## Prerequisites

- Node.js >= 20.0.0
- npm >= 10.0.0

## Getting Started

```bash
# Install dependencies
npm install

# Set up environment
cp .env.example .env.local
# Edit .env.local to configure API URLs

# Start dev server
npm run dev
```

The app will start at `http://localhost:3000`.

## Project Structure

```
src/
├── __tests__/          # Vitest test files
├── api/                # API client layer (axios-based)
│   └── client.ts       # Axios instance, interceptors, error types
├── app/                # Next.js App Router
│   ├── (main)/         # Authenticated routes group
│   │   ├── admin/      # Admin feature pages (placeholders in 7.1)
│   │   ├── dashboard/  # Dashboard page
│   │   └── layout.tsx  # Sidebar + topbar layout
│   ├── error.tsx       # Global error boundary
│   ├── layout.tsx      # Root layout (fonts, providers)
│   ├── not-found.tsx   # 404 page
│   └── page.tsx        # Redirect to /dashboard
├── components/
│   ├── providers/      # React context providers (Query, Theme, Auth, Permission, Tenant)
│   ├── shared/         # Business components (StatusBadge, MoneyAmount, ConfirmDialog, states, permission-guards)
│   └── ui/             # shadcn-style primitives (Button, Card, Badge)
├── config/             # App constants (API URLs, colors, pagination)
├── hooks/              # Custom hooks (usePermission, useTenant)
├── lib/                # Utilities (cn, formatCurrency, maskCard, etc.)
├── stores/             # Zustand stores (auth.store with persistence)
├── styles/             # Global CSS (shadcn vars, Tailwind base)
└── types/              # TypeScript type definitions (domain models)
```

## Available Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start Next.js dev server |
| `npm run dev:turbo` | Start with Turbopack |
| `npm run build` | Production build |
| `npm run start` | Start production server |
| `npm run lint` | Run ESLint |
| `npm run lint:fix` | ESLint auto-fix |
| `npm run format` | Prettier format |
| `npm run format:check` | Prettier check |
| `npm run type-check` | TypeScript type checking (`tsc --noEmit`) |
| `npm run test` | Run Vitest tests |
| `npm run test:coverage` | Run tests with coverage |

## Code Conventions

### React Components

- Client components use `'use client'` directive at the top of the file
- Server components (default in Next.js 15) don't need a directive
- Use TypeScript strict mode
- Prefer functional components with explicit prop interfaces

### State Management

- **Server state** → @tanstack/react-query (data fetching, caching)
- **Client state** → Zustand (auth, UI state)
- **Local state** → useState (form inputs, modals, filters)
- **URL state** → Next.js searchParams (pagination, search queries)

### Styling

- Tailwind CSS utilities for all styles
- CSS custom properties for theme (light/dark mode via shadcn)
- `cn()` utility (clsx + tailwind-merge) for conditional classes

### API Calls

- Use typed API functions from `@/api/client`
- All API calls go through the axios instance (auth, tenant, app headers injected automatically)
- Handle errors via try/catch — errors are typed `ApiError`

### Permissions

- Import guards from `@/components/shared/permission-guards`
- Wrap routes with `RoutePermissionGuard` for page-level access control
- Wrap actions with `PermissionGuard` for action-level access control

## Adding a New Feature Page

1. Create the page file: `src/app/(main)/admin/<feature>/page.tsx`
2. Add `'use client'` directive
3. Wrap content in `RoutePermissionGuard` with required permissions
4. Add the route to sidebar in `src/app/(main)/layout.tsx`
5. Update `docs/routes.md`

## Testing

```bash
# Run all tests
npm run test

# Run specific test file
npm run test -- src/__tests__/auth-store.test.ts

# Watch mode
npm run test -- --watch

# Coverage
npm run test:coverage
```

---

> Last updated: 2026-07-07
