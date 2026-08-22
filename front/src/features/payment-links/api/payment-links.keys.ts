export const paymentLinkKeys = {
  all: ['payment-links'] as const,
  lists: () => [...paymentLinkKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) =>
    params
      ? [...paymentLinkKeys.lists(), params] as const
      : [...paymentLinkKeys.lists()] as const,
  details: () => [...paymentLinkKeys.all, 'detail'] as const,
  detail: (id: string) => [...paymentLinkKeys.details(), id] as const,
};
