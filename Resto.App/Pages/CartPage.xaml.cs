using Resto.App.Models;
using Resto.App.Services;

namespace Resto.App.Pages;

public partial class CartPage : ContentPage
{
    private readonly AppDataService _data;
    private string _selectedPayment = "COP";

    public CartPage(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _data.OnChange += RefreshCart;
        RefreshCart();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _data.OnChange -= RefreshCart;
    }

    private void RefreshCart()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var hasItems = _data.Cart.Any();
            EmptyState.IsVisible     = !hasItems;
            CartScrollView.IsVisible =  hasItems;
            CheckoutFooter.IsVisible =  hasItems;

            if (hasItems)
            {
                CartList.ItemsSource = null;
                CartList.ItemsSource = _data.Cart.ToList();
                SubtotalLabel.Text   = $"₱{_data.CartTotal:N2}";
                TotalLabel.Text      = $"₱{_data.CartTotal:N2}";
                CartSubtitle.Text    = $"{_data.CartCount} item(s) • ₱{_data.CartTotal:N2}";
            }
            else
            {
                CartSubtitle.Text = "Your cart is empty";
            }
        });
    }

    private void OnDecreaseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
            _data.UpdateCartQuantity(item, -1);
    }

    private void OnIncreaseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
            _data.UpdateCartQuantity(item, +1);
    }

    private void OnSelectCard(object sender, TappedEventArgs e)
    {
        _selectedPayment        = "Card";
        CardBtn.BackgroundColor = Color.FromArgb("#fce4e4");
        CopBtn.BackgroundColor  = Colors.White;
    }

    private void OnSelectCOP(object sender, TappedEventArgs e)
    {
        _selectedPayment        = "COP";
        CopBtn.BackgroundColor  = Color.FromArgb("#fce4e4");
        CardBtn.BackgroundColor = Colors.White;
    }

    private async void OnPlaceOrderClicked(object sender, EventArgs e)
    {
        if (!_data.Cart.Any())
        {
            await DisplayAlert("Empty Cart", "Add items to your cart before ordering.", "OK");
            return;
        }

        if (_selectedPayment == "Card")
        {
            // Snapshot the cart, then navigate to the payment page
            var orderRef = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            var amount   = _data.CartTotal;
            _data.SnapshotCartForPayment();
            await Shell.Current.GoToAsync(
                $"PaymentPage?orderRef={Uri.EscapeDataString(orderRef)}&amount={amount}");
            return;
        }

        // COP flow — place immediately
        bool confirm = await DisplayAlert(
            "Confirm Order",
            $"Total: ₱{_data.CartTotal:N2}\nPayment: Cash on Pick-Up\n\nPlace this order?",
            "Place Order", "Cancel");

        if (!confirm) return;

        PlaceOrderBtn.IsEnabled = false;
        PlaceOrderBtn.Text      = "Placing Order...";

        var order = _data.PlaceOrder(_selectedPayment);

        PlaceOrderBtn.IsEnabled = true;
        PlaceOrderBtn.Text      = "Place Order";

        await DisplayAlert("Order Placed! 🎉",
            $"Order #{order.Id} placed successfully!\nYou'll receive updates on your status.",
            "View Orders");

        await Shell.Current.GoToAsync("//OrdersPage");
    }

    private void OnBrowseMenuClicked(object sender, EventArgs e)
        => Shell.Current.GoToAsync("//MenuPage");
}
