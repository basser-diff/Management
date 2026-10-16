using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Resto.Client.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    // ============================================================
    // ENCRYPTED DATABASE FIELDS
    // ============================================================

    [MaxLength(1000)]
    public string CustomerNameEnc { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string TransactionRefEnc { get; set; } = string.Empty;

    // ============================================================
    // DISPLAY-ONLY FIELDS
    // These are NOT stored in SQL Server.
    // DataService fills them after decrypting the encrypted fields.
    // ============================================================

    [NotMapped]
    public string CustomerName { get; set; } = string.Empty;

    [NotMapped]
    public string TransactionRef { get; set; } = string.Empty;

    // ============================================================
    // PAYMENT INFORMATION
    // ============================================================

    [MaxLength(50)]
    public string Method { get; set; } = "Card";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime TransactionDate { get; set; } = DateTime.Now;
}