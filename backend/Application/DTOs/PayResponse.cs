using Parbad;

namespace Application.DTOs;

/// <summary>
/// Response from payment initiation containing gateway details.
/// </summary>
public class PayResponse
{
    public Guid PaymentId { get; set; }
    public long TrackingNumber { get; set; }
    public string GatewayUrl { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? Message { get; set; }
    
    /// <summary>
    /// Gateway transporter for client-side form submission (optional).
    /// Only available when gateway requires form-based redirect.
    /// </summary>
    public IPaymentRequestResult? PaymentRequestResult { get; set; }
}