using Minisource.Common.Domain;

namespace Domain.Entities;

/// <summary>
/// Tenant-specific notification settings for payment events.
/// All notification types are enabled by default when no override row exists.
/// Each row represents a specific notification key + channel + recipient type combination.
/// </summary>
public class PaymentNotificationSetting : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string ApplicationCode { get; set; } = "payment";

    /// <summary>e.g. payment_succeeded, refund_requested_admin</summary>
    public string NotificationKey { get; set; } = string.Empty;

    /// <summary>in_app, sms, email, push</summary>
    public string Channel { get; set; } = "in_app";

    /// <summary>user or admin</summary>
    public string RecipientType { get; set; } = "user";

    /// <summary>When false, this notification type is suppressed for this tenant.</summary>
    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public PaymentNotificationSetting()
    {
        Id = Guid.NewGuid();
    }

    public static PaymentNotificationSetting Create(
        Guid tenantId,
        string notificationKey,
        string channel = "in_app",
        string recipientType = "admin",
        bool isEnabled = true)
    {
        return new PaymentNotificationSetting
        {
            TenantId = tenantId,
            NotificationKey = notificationKey,
            Channel = channel,
            RecipientType = recipientType,
            IsEnabled = isEnabled
        };
    }
}
