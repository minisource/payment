import { apiClient } from '@/api/client';
import type {
  NotificationListResponse,
  UnreadCountResponse,
  MarkAllAsReadResponse,
  NotificationSettingsListResponse,
  NotificationKeysResponse,
  ListNotificationsParams,
  NotificationSettingDto,
} from '../types/notification.types';

const BASE = '/api/v1/admin';

export async function listNotifications(
  params: ListNotificationsParams,
  signal?: AbortSignal,
): Promise<NotificationListResponse> {
  const { data } = await apiClient.get<NotificationListResponse>(`${BASE}/notifications`, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
      ...(params.status && { status: params.status }),
      ...(params.channel && { channel: params.channel }),
      ...(params.userId && { userId: params.userId }),
      ...(params.search && { search: params.search }),
      ...(params.from && { from: params.from }),
      ...(params.to && { to: params.to }),
      ...(params.tenantScope === 'all' && { tenant_scope: 'all' }),
    },
    signal,
  });
  return data;
}

export async function markAsRead(notificationId: string): Promise<void> {
  await apiClient.put(`${BASE}/notifications/${notificationId}/read`);
}

export async function markAllAsRead(): Promise<MarkAllAsReadResponse> {
  const { data } = await apiClient.post<MarkAllAsReadResponse>(`${BASE}/notifications/read-all`);
  return data;
}

export async function getUnreadCount(signal?: AbortSignal): Promise<UnreadCountResponse> {
  const { data } = await apiClient.get<UnreadCountResponse>(`${BASE}/notifications/unread-count`, { signal });
  return data;
}

export async function listNotificationSettings(
  signal?: AbortSignal,
): Promise<NotificationSettingsListResponse> {
  const { data } = await apiClient.get<NotificationSettingsListResponse>(
    `${BASE}/notification-settings`,
    { signal },
  );
  return data;
}

export async function updateNotificationSetting(
  notificationKey: string,
  isEnabled: boolean,
  channel = 'in_app',
  recipientType = 'admin',
): Promise<NotificationSettingDto> {
  const { data } = await apiClient.put<NotificationSettingDto>(
    `${BASE}/notification-settings/${notificationKey}`,
    { isEnabled, channel, recipientType },
  );
  return data;
}

export async function listNotificationKeys(signal?: AbortSignal): Promise<NotificationKeysResponse> {
  const { data } = await apiClient.get<NotificationKeysResponse>(
    `${BASE}/notification-settings/keys`,
    { signal },
  );
  return data;
}
