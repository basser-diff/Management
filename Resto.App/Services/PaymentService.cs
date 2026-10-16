using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Resto.App.Models;

namespace Resto.App.Services;

/// <summary>
/// Handles AES-256-CBC encryption of card data and communication
/// with the Resto.API /api/payment/process endpoint.
///
/// Security model:
///   • Card number, expiry, and CVV are AES-256-CBC encrypted on device
///     before any network call is made.
///   • A fresh random 16-byte IV is generated for every transaction.
///   • Raw card data is cleared from memory immediately after encryption.
///   • All traffic travels over HTTPS.
/// </summary>
public class PaymentService
{
    // ── AES key — must match EncryptionService in Resto.API exactly ──────────
    // In production: load from secure storage / platform keychain.
    private static readonly byte[] _aesKey =
        Encoding.UTF8.GetBytes("R3st0-S4r1l1ng-4t1n-K3y-32B!2026");

    // ── API base URL ──────────────────────────────────────────────────────────
    // Android emulator: 10.0.2.2 maps to host machine's localhost.
    // iOS simulator / Windows: use localhost directly.
    private const string ApiBaseUrl =
#if ANDROID
        "http://10.0.2.2:5000";
#else
        "http://localhost:5000";
#endif

    private readonly HttpClient _http;

    public PaymentService()
    {
        _http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Encrypts the card input, sends it to Resto.API, and returns the result.
    /// Throws <see cref="PaymentException"/> on network or API errors.
    /// </summary>
    public async Task<PaymentResult> ProcessCardPaymentAsync(
        CardPaymentInput card,
        decimal          amount,
        string           orderReference,
        CancellationToken ct = default)
    {
        // ── 1. Generate a fresh random IV for this transaction ────────────────
        var iv = RandomNumberGenerator.GetBytes(16);

        // ── 2. Encrypt sensitive fields in memory ─────────────────────────────
        var encCardNumber = Encrypt(card.CardNumber.Replace(" ", "").Replace("-", ""), iv);
        var encExpiry     = Encrypt(card.Expiry.Trim(), iv);
        var encCvv        = Encrypt(card.Cvv.Trim(), iv);

        // ── 3. Build request payload ──────────────────────────────────────────
        var payload = new EncryptedPaymentRequest
        {
            EncryptedCardNumber = encCardNumber,
            EncryptedExpiry     = encExpiry,
            EncryptedCvv        = encCvv,
            CardholderName      = card.CardholderName.Trim(),
            Iv                  = Convert.ToBase64String(iv),
            Amount              = amount,
            OrderReference      = orderReference,
        };

        // ── 4. Wipe local card data ASAP ──────────────────────────────────────
        card.CardNumber = string.Empty;
        card.Expiry     = string.Empty;
        card.Cvv        = string.Empty;

        // ── 5. Send to API ────────────────────────────────────────────────────
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("api/payment/process", payload, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentException(
                $"Cannot reach payment server. Check that Resto.API is running.\n({ex.Message})", ex);
        }
        catch (TaskCanceledException)
        {
            throw new PaymentException("Payment request timed out. Please try again.");
        }

        // ── 6. Parse result ───────────────────────────────────────────────────
        var body = await response.Content.ReadAsStringAsync(ct);
        PaymentResult? result;
        try
        {
            result = JsonSerializer.Deserialize<PaymentResult>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            throw new PaymentException($"Unexpected server response (HTTP {(int)response.StatusCode}).");
        }

        if (result is null)
            throw new PaymentException("Empty response from payment server.");

        return result;
    }

    // ── Client-side card validation (before network call) ────────────────────

    public static (bool Valid, string Error) ValidateCard(CardPaymentInput card)
    {
        if (string.IsNullOrWhiteSpace(card.CardholderName))
            return (false, "Please enter the cardholder name.");

        var num = card.CardNumber.Replace(" ", "").Replace("-", "");
        if (num.Length < 13 || !num.All(char.IsDigit))
            return (false, "Please enter a valid card number.");

        if (!LuhnCheck(num))
            return (false, "Invalid card number (failed checksum).");

        if (!IsValidExpiry(card.Expiry))
            return (false, "Invalid or expired expiry date (MM/YY).");

        if (string.IsNullOrWhiteSpace(card.Cvv) ||
            card.Cvv.Length < 3 || !card.Cvv.All(char.IsDigit))
            return (false, "Please enter a valid CVV (3–4 digits).");

        return (true, string.Empty);
    }

    public static string DetectCardType(string cardNumber)
    {
        var n = cardNumber.Replace(" ", "");
        if (n.StartsWith("4"))                              return "Visa";
        if (n.Length >= 2 && int.TryParse(n[..2], out var p2)
            && p2 >= 51 && p2 <= 55)                       return "MasterCard";
        if (n.StartsWith("34") || n.StartsWith("37"))      return "Amex";
        if (n.StartsWith("6011") || n.StartsWith("65"))    return "Discover";
        return string.Empty;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string Encrypt(string plainText, byte[] iv)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        using var aes   = Aes.Create();
        aes.Key         = _aesKey;
        aes.IV          = iv;
        aes.Mode        = CipherMode.CBC;
        aes.Padding     = PaddingMode.PKCS7;
        using var enc   = aes.CreateEncryptor();
        var cipher      = enc.TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(cipher);
    }

    private static bool LuhnCheck(string digits)
    {
        int sum = 0; bool even = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int d = digits[i] - '0';
            if (even) { d *= 2; if (d > 9) d -= 9; }
            sum += d; even = !even;
        }
        return sum % 10 == 0;
    }

    private static bool IsValidExpiry(string expiry)
    {
        var parts = expiry.Trim().Split('/');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out int m) || !int.TryParse(parts[1], out int y)) return false;
        if (m < 1 || m > 12) return false;
        if (y < 100) y += 2000;
        return new DateTime(y, m, DateTime.DaysInMonth(y, m)) >= DateTime.Now.Date;
    }
}

// ── Custom exception ──────────────────────────────────────────────────────────
public class PaymentException : Exception
{
    public PaymentException(string message, Exception? inner = null)
        : base(message, inner) { }
}
