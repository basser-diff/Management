namespace Resto.App.Models;

// ── Card input (local only — never sent raw) ──────────────────────────────────
public class CardPaymentInput
{
    public string CardNumber     { get; set; } = string.Empty;
    public string Expiry         { get; set; } = string.Empty;   // MM/YY
    public string Cvv            { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
}

// ── What the app sends to Resto.API (all sensitive fields encrypted) ──────────
public class EncryptedPaymentRequest
{
    public string  EncryptedCardNumber { get; set; } = string.Empty;
    public string  EncryptedExpiry     { get; set; } = string.Empty;
    public string  EncryptedCvv        { get; set; } = string.Empty;
    public string  CardholderName      { get; set; } = string.Empty;  // plain
    public string  Iv                  { get; set; } = string.Empty;  // Base64
    public decimal Amount              { get; set; }
    public string  OrderReference      { get; set; } = string.Empty;
}

// ── What Resto.API returns ────────────────────────────────────────────────────
public class PaymentResult
{
    public bool     Success          { get; set; }
    public string   TransactionId    { get; set; } = string.Empty;
    public string   Message          { get; set; } = string.Empty;
    public string   MaskedCardNumber { get; set; } = string.Empty;
    public string   CardType         { get; set; } = string.Empty;
    public decimal  Amount           { get; set; }
    public DateTime ProcessedAt      { get; set; }
}
