using Domain.Enums;

namespace Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public long TrackingNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public PaymentStatus Status { get; set; }
    public string Gateway { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public decimal CreditApplied { get; set; }
    public decimal AmountDue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}