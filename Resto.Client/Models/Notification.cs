namespace Resto.Client.Models;

public class AppNotification
{
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Message   { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool     IsRead    { get; set; }
    public NotificationType Type { get; set; }
}

public enum NotificationType
{
    NewOrder,
    PaymentConfirmed,
    LowStock,
    OrderReady,
    General
}
