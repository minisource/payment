namespace Application.DTOs;

public class PaymentLogDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}