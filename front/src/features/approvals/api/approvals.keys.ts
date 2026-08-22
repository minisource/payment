export const approvalKeys = {
  all: ['approvals'] as const,
  lists: () => [...approvalKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) =>
    params
      ? [...approvalKeys.lists(), params] as const
      : [...approvalKeys.lists()] as const,
  details: () => [...approvalKeys.all, 'detail'] as const,
  detail: (id: string) => [...approvalKeys.details(), id] as const,
};
