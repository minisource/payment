export interface NotificationDto {
  id: string;
  tenantId?: string;
  userId: string;
  type: string;
  status: string;
  priority: string;
  subject?: string;
  body: string;
  templateKey?: string;
  locale?: string;
  retryCount: number;
  maxRetries: number;
  errorMessage?: string;
  provider?: string;
  seenAt?: string;
  readAt?: string;
  clickedAt?: string;
  sentAt?: string;
  deliveredAt?: string;
  failedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface NotificationListResponse {
  data: NotificationDto[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UnreadCountResponse {
  userId: string;
  unreadCount: number;
}

export interface MarkAllAsReadResponse {
  message: string;
  userId: string;
  updatedCount: number;
}

export interface NotificationSettingDto {
  notification_key: string;
  channel: string;
  recipient_type: string;
  description: string;
  is_enabled: boolean;
  tenant_id?: string;
  application_code: string;
}

export interface NotificationSettingsListResponse {
  items: NotificationSettingDto[];
}

export interface NotificationKeyInfo {
  key: string;
  recipient_type: string;
  description: string;
}

export interface NotificationKeysResponse {
  keys: NotificationKeyInfo[];
}

export interface ListNotificationsParams {
  page?: number;
  pageSize?: number;
  status?: string;
  channel?: string;
  userId?: string;
  search?: string;
  from?: string;
  to?: string;
  tenantId?: string;
  tenantScope?: string;
}
