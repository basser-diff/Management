using System.ComponentModel.DataAnnotations;

namespace Resto.API.Models;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2
}

public class CreatePaymentRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string Method { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string? TransactionReference { get; set; }

    public string? CardNumber { get; set; }
}

public class PaymentResponse
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? TransactionReference { get; set; }
}