using Resto.App.Models;
using Resto.App.Services;

namespace Resto.App.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly AppDataService _data;

    public NotificationsPage(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _data.OnChange += RefreshNotifications;
        RefreshNotifications();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _data.OnChange -= RefreshNotifications;
    }

    private void RefreshNotifications()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var notifs    = _data.Notifications;
            var hasNotifs = notifs.Any();
            EmptyState.IsVisible  = !hasNotifs;
            NotifList.IsVisible   =  hasNotifs;
            NotifList.ItemsSource = null;
            NotifList.ItemsSource = notifs.ToList();

            var unread = _data.UnreadCount;
            UnreadLabel.Text = unread > 0
                ? $"{unread} unread notification{(unread == 1 ? "" : "s")}"
                : "All caught up!";
        });
    }

    private void OnMarkAllReadClicked(object sender, EventArgs e)
        => _data.MarkAllRead();

    private void OnNotifTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is CustomerNotification notif)
            _data.MarkRead(notif.Id);
    }

    private void OnDeleteSwipe(object sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is CustomerNotification notif)
            _data.DeleteNotification(notif.Id);
    }
}
