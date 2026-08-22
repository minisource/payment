export const approvalPolicyKeys = {
  all: ['approval-policies'] as const,
  lists: () => [...approvalPolicyKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) =>
    params
      ? [...approvalPolicyKeys.lists(), params] as const
      : [...approvalPolicyKeys.lists()] as const,
  details: () => [...approvalPolicyKeys.all, 'detail'] as const,
  detail: (id: string) => [...approvalPolicyKeys.details(), id] as const,
};
