using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class VerifyRequest
{
    [Required]
    public long TrackingNumber { get; set; }
}