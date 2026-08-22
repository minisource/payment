import type { ListNotificationsParams } from '../types/notification.types';

export const notificationKeys = {
  all: ['notifications'] as const,
  list: (params: ListNotificationsParams) => [...notificationKeys.all, 'list', params] as const,
  unreadCount: () => [...notificationKeys.all, 'unread-count'] as const,
  settings: () => [...notificationKeys.all, 'settings'] as const,
  settingsKeys: () => [...notificationKeys.all, 'settings-keys'] as const,
};
