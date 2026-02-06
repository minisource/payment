using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Response from payment verification.
/// </summary>
public class VerifyResponse
{
    public PaymentStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? TransactionCode { get; set; }
    public long TrackingNumber { get; set; }
    public decimal Amount { get; set; }
    public string? Gateway { get; set; }
}