import { useQuery } from '@tanstack/react-query';
import { notificationKeys } from './notifications.keys';
import * as api from './notifications.api';
import type { ListNotificationsParams } from '../types/notification.types';

export function useNotificationsQuery(params: ListNotificationsParams, enabled = true) {
  return useQuery({
    queryKey: notificationKeys.list(params),
    queryFn: ({ signal }) => api.listNotifications(params, signal),
    enabled,
    staleTime: 15_000,
  });
}

export function useUnreadCountQuery(enabled = true) {
  return useQuery({
    queryKey: notificationKeys.unreadCount(),
    queryFn: ({ signal }) => api.getUnreadCount(signal),
    enabled,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}

export function useNotificationSettingsQuery(enabled = true) {
  return useQuery({
    queryKey: notificationKeys.settings(),
    queryFn: ({ signal }) => api.listNotificationSettings(signal),
    enabled,
    staleTime: 60_000,
  });
}

export function useNotificationKeysQuery(enabled = true) {
  return useQuery({
    queryKey: notificationKeys.settingsKeys(),
    queryFn: ({ signal }) => api.listNotificationKeys(signal),
    enabled,
    staleTime: 5 * 60_000,
  });
}
