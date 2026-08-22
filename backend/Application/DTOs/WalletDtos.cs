using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "IRR";
    public DateTime UpdatedAt { get; set; }
}

public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public WalletTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreditWalletRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}

public class DebitWalletRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}
