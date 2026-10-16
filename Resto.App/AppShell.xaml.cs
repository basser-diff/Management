using Resto.App.Pages;
using Resto.App.Services;

namespace Resto.App;

public partial class AppShell : Shell
{
    private readonly AppDataService _data;

    public AppShell(AppDataService data)
    {
        InitializeComponent();
        _data = data;
        _data.OnChange += UpdateBadges;

        // Register PaymentPage as a modal route (not a tab)
        Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
        Routing.RegisterRoute("PaymentPage",        typeof(PaymentPage));

        UpdateBadges();
    }

    private void UpdateBadges()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Shell.SetTabBarIsVisible(this, true);
            if (NotificationsTab != null)
                NotificationsTab.Title = _data.UnreadCount > 0
                    ? $"Alerts ({_data.UnreadCount})"
                    : "Alerts";
        });
    }
}
