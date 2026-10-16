using Resto.App.Services;

namespace Resto.App.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly AppDataService _data;

    public OrdersPage(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _data.OnChange += RefreshOrders;
        RefreshOrders();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _data.OnChange -= RefreshOrders;
    }

    private void RefreshOrders()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var hasOrders = _data.Orders.Any();
            EmptyState.IsVisible  = !hasOrders;
            OrdersList.IsVisible  =  hasOrders;
            OrdersList.ItemsSource = null;
            OrdersList.ItemsSource = _data.Orders.ToList();
        });
    }

    private void OnBrowseMenuClicked(object sender, EventArgs e)
        => Shell.Current.GoToAsync("//MenuPage");
}
