using Resto.App.Models;
using Resto.App.Services;

namespace Resto.App.Pages;

/// <summary>
/// Payment page — collects card details, encrypts them client-side,
/// sends to Resto.API, and shows the result.
/// </summary>
[QueryProperty(nameof(OrderReference), "orderRef")]
[QueryProperty(nameof(AmountStr),      "amount")]
public partial class PaymentPage : ContentPage
{
    private readonly PaymentService  _paymentService;
    private readonly AppDataService  _data;

    // ── Query properties (passed via Shell navigation) ────────────────────────
    public string OrderReference { get; set; } = string.Empty;
    public string AmountStr      { get; set; } = "0";

    private decimal Amount => decimal.TryParse(AmountStr, out var a) ? a : 0m;

    public PaymentPage(PaymentService paymentService, AppDataService data)
    {
        InitializeComponent();
        _paymentService = paymentService;
        _data           = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AmountLabel.Text = $"Total: ₱{Amount:N2}  •  Order {OrderReference}";
        PayButton.Text   = $"Pay  ₱{Amount:N2}";
        ShowForm();
    }

    // ── Card number input — live formatting & preview ─────────────────────────
    private bool _formatting = false;

    private void OnCardNumberChanged(object sender, TextChangedEventArgs e)
    {
        if (_formatting) return;
        _formatting = true;

        var raw = (e.NewTextValue ?? string.Empty)
            .Replace(" ", "").Replace("-", "")
            .Where(char.IsDigit)
            .Take(16);
        var digits = new string(raw.ToArray());

        // Format as groups of 4
        var formatted = string.Join(" ",
            Enumerable.Range(0, (digits.Length + 3) / 4)
                      .Select(i => digits.Substring(i * 4, Math.Min(4, digits.Length - i * 4))));

        CardNumberEntry.Text = formatted;
        _formatting = false;

        // Update preview
        CardNumberPreview.Text = formatted.Length > 0
            ? formatted.PadRight(19, '•').Insert(4, " ").Insert(9, " ").Insert(14, " ")
            : "•••• •••• •••• ••••";
        CardNumberPreview.Text = digits.Length == 0
            ? "•••• •••• •••• ••••"
            : formatted + (formatted.Length < 19 ? new string('•', 19 - formatted.Length) : "");

        // Card type
        var type = PaymentService.DetectCardType(digits);
        CardTypePreview.Text   = type;
        CardNetworkIcon.Text   = type switch
        {
            "Visa"       => "💳",
            "MasterCard" => "💳",
            "Amex"       => "💳",
            _            => "",
        };

        ClearFieldError(CardNumberBorder, CardNumberError);
    }

    private void OnHolderNameChanged(object sender, TextChangedEventArgs e)
    {
        HolderPreview.Text = string.IsNullOrWhiteSpace(e.NewTextValue)
            ? "FULL NAME"
            : e.NewTextValue.ToUpperInvariant();
        ClearFieldError(NameBorder, NameError);
    }

    private void OnExpiryChanged(object sender, TextChangedEventArgs e)
    {
        var raw = (e.NewTextValue ?? string.Empty).Replace("/", "");
        if (raw.Length > 2 && !e.NewTextValue.Contains('/'))
            ExpiryEntry.Text = raw.Insert(2, "/");

        ExpiryPreview.Text = string.IsNullOrWhiteSpace(ExpiryEntry.Text)
            ? "MM/YY"
            : ExpiryEntry.Text;

        ClearFieldError(ExpiryBorder, ExpiryError);
    }

    private void OnCvvChanged(object sender, TextChangedEventArgs e)
        => ClearFieldError(CvvBorder, CvvError);

    private async void OnCvvInfoTapped(object sender, TappedEventArgs e)
        => await DisplayAlert("CVV / CVC",
            "The 3-digit security code on the back of your card (Visa, MasterCard).\nAmex uses a 4-digit code on the front.",
            "OK");

    // ── Pay button ────────────────────────────────────────────────────────────
    private async void OnPayClicked(object sender, EventArgs e)
    {
        if (!ValidateForm()) return;

        var card = new CardPaymentInput
        {
            CardNumber     = CardNumberEntry.Text  ?? string.Empty,
            Expiry         = ExpiryEntry.Text       ?? string.Empty,
            Cvv            = CvvEntry.Text           ?? string.Empty,
            CardholderName = CardholderEntry.Text    ?? string.Empty,
        };

        ShowProcessing();

        try
        {
            var result = await _paymentService.ProcessCardPaymentAsync(
                card, Amount, OrderReference);

            if (result.Success)
            {
                // Place the order in the local data service as Card/Paid
                _data.PlaceOrderAfterPayment(
     paymentMethod: "Card",
     transactionId: result.TransactionId);

                ShowSuccess(result);
            }
            else
            {
                ShowFailure(result.Message);
            }
        }
        catch (PaymentException pex)
        {
            ShowFailure(pex.Message);
        }
        catch (Exception ex)
        {
            ShowFailure($"Unexpected error: {ex.Message}");
        }
    }

    // ── Result handlers ───────────────────────────────────────────────────────

    private void ShowSuccess(PaymentResult result)
    {
        FormView.IsVisible        = false;
        ProcessingOverlay.IsVisible = false;
        FailureView.IsVisible     = false;
        SuccessView.IsVisible     = true;

        SuccessAmountLabel.Text  = $"₱{result.Amount:N2}";
        TxnIdLabel.Text          = result.TransactionId;
        MaskedCardLabel.Text     = result.MaskedCardNumber;
        CardTypeLabel.Text       = result.CardType;
        ProcessedAtLabel.Text    = result.ProcessedAt.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt");
    }

    private void ShowFailure(string message)
    {
        ProcessingOverlay.IsVisible = false;
        FormView.IsVisible          = true;
        SuccessView.IsVisible       = false;
        FailureView.IsVisible       = true;
        FailureMessageLabel.Text    = message;
    }

    private void ShowProcessing()
    {
        HideAllErrors();
        FormView.IsVisible          = false;
        ProcessingOverlay.IsVisible = true;
        SuccessView.IsVisible       = false;
        FailureView.IsVisible       = false;
    }

    private void ShowForm()
    {
        FormView.IsVisible          = true;
        ProcessingOverlay.IsVisible = false;
        SuccessView.IsVisible       = false;
        FailureView.IsVisible       = false;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private async void OnViewOrdersClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//OrdersPage");

    private void OnTryAgainClicked(object sender, EventArgs e)
    {
        FailureView.IsVisible = false;
        ShowForm();
    }

    private async void OnSwitchToCOPClicked(object sender, EventArgs e)
    {
        _data.PlaceOrderAfterPayment("COP", string.Empty);
        await DisplayAlert("Order Placed",
            "Your order has been placed with Cash on Pick-Up payment.", "OK");
        await Shell.Current.GoToAsync("//OrdersPage");
    }

    // ── Form validation ───────────────────────────────────────────────────────

    private bool ValidateForm()
    {
        HideAllErrors();
        bool valid = true;

        var cardInput = new CardPaymentInput
        {
            CardNumber     = CardNumberEntry.Text  ?? string.Empty,
            Expiry         = ExpiryEntry.Text       ?? string.Empty,
            Cvv            = CvvEntry.Text           ?? string.Empty,
            CardholderName = CardholderEntry.Text    ?? string.Empty,
        };

        // Individual field checks for inline feedback
        if (string.IsNullOrWhiteSpace(cardInput.CardholderName))
        {
            ShowFieldError(NameBorder, NameError, "Cardholder name is required.");
            valid = false;
        }

        var num = cardInput.CardNumber.Replace(" ", "");
        if (num.Length < 13 || !num.All(char.IsDigit))
        {
            ShowFieldError(CardNumberBorder, CardNumberError, "Enter a valid card number.");
            valid = false;
        }
        else
        {
            var (luhnOk, luhnErr) = PaymentService.ValidateCard(cardInput);
            if (!luhnOk && luhnErr.Contains("checksum"))
            {
                ShowFieldError(CardNumberBorder, CardNumberError, luhnErr);
                valid = false;
            }
        }

        if (string.IsNullOrWhiteSpace(cardInput.Expiry) || !cardInput.Expiry.Contains('/'))
        {
            ShowFieldError(ExpiryBorder, ExpiryError, "Enter expiry as MM/YY.");
            valid = false;
        }
        else
        {
            var parts = cardInput.Expiry.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out _) || !int.TryParse(parts[1], out _))
            {
                ShowFieldError(ExpiryBorder, ExpiryError, "Invalid expiry format.");
                valid = false;
            }
        }

        if (string.IsNullOrWhiteSpace(cardInput.Cvv) ||
            cardInput.Cvv.Length < 3 || !cardInput.Cvv.All(char.IsDigit))
        {
            ShowFieldError(CvvBorder, CvvError, "Enter a valid CVV (3–4 digits).");
            valid = false;
        }

        return valid;
    }

    private static void ShowFieldError(Border border, Label errorLabel, string message)
    {
        border.Stroke         = Color.FromArgb("#d32f2f");
        errorLabel.Text       = message;
        errorLabel.IsVisible  = true;
    }

    private static void ClearFieldError(Border border, Label errorLabel)
    {
        border.Stroke        = Color.FromArgb("#ddd");
        errorLabel.IsVisible = false;
    }

    private void HideAllErrors()
    {
        GlobalErrorBanner.IsVisible = false;
        ClearFieldError(CardNumberBorder, CardNumberError);
        ClearFieldError(NameBorder, NameError);
        ClearFieldError(ExpiryBorder, ExpiryError);
        ClearFieldError(CvvBorder, CvvError);
    }
}
