export const payoutAccountKeys = {
  all: ['payout-accounts'] as const,
  lists: () => [...payoutAccountKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) =>
    params
      ? [...payoutAccountKeys.lists(), params] as const
      : [...payoutAccountKeys.lists()] as const,
  details: () => [...payoutAccountKeys.all, 'detail'] as const,
  detail: (id: string) => [...payoutAccountKeys.details(), id] as const,
};
