import { useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationKeys } from './notifications.keys';
import * as api from './notifications.api';
import { toast } from 'sonner';

export function useMarkAsReadMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (notificationId: string) => api.markAsRead(notificationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
    onError: () => {
      toast.error('Failed to mark notification as read');
    },
  });
}

export function useMarkAllAsReadMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => api.markAllAsRead(),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      toast.success(`Marked ${data.updatedCount} notifications as read`);
    },
    onError: () => {
      toast.error('Failed to mark all as read');
    },
  });
}

export function useUpdateNotificationSettingMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      notificationKey,
      isEnabled,
      channel = 'in_app',
      recipientType = 'admin',
    }: {
      notificationKey: string;
      isEnabled: boolean;
      channel?: string;
      recipientType?: string;
    }) => api.updateNotificationSetting(notificationKey, isEnabled, channel, recipientType),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.settings() });
      toast.success('Notification setting updated');
    },
    onError: () => {
      toast.error('Failed to update notification setting');
    },
  });
}
