export const tenantKeys = {
  all: ['tenants'] as const,
  list: () => [...tenantKeys.all, 'list'] as const,
};
