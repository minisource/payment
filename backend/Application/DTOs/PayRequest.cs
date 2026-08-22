using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PayRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "IRR";

    public string? Gateway { get; set; }

    [Required]
    [Url]
    public string CallbackUrl { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ReturnUrl { get; set; } = string.Empty;

    public string Metadata { get; set; } = string.Empty;

    public bool UseWallet { get; set; } = false;

    public string? UserId { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate payments.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}