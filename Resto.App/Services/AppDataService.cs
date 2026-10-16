using Resto.App.Data;
using Resto.App.Models;

namespace Resto.App.Services;

/// <summary>
/// Customer app data service backed by a local SQLite database.
/// Menu, orders, and notifications all persist across app restarts.
/// Cart remains in-memory (session-only by design).
/// </summary>
public class AppDataService
{
    private readonly AppDatabase _db;

    public event Action? OnChange;
    private void Notify() => OnChange?.Invoke();

    public AppDataService(AppDatabase db) => _db = db;

    // ── Initialisation ────────────────────────────────────────────────────────

    public async Task InitialiseAsync()
    {
        await _db.InitialiseAsync();

        // Load initial data into memory caches
        await RefreshMenuAsync();
        await RefreshOrdersAsync();
        await RefreshNotificationsAsync();
    }

    // ── Menu ──────────────────────────────────────────────────────────────────

    private List<FoodItem> _menu = new();

    public List<FoodItem> Menu => _menu;

    public List<string> Categories
        => _menu.Select(m => m.Category).Distinct().OrderBy(c => c).ToList();

    public async Task RefreshMenuAsync()
    {
        var entities = await _db.GetMenuAsync();
        _menu = entities.Select(ToModel).ToList();
        Notify();
    }

    private static FoodItem ToModel(FoodItemEntity e) => new()
    {
        Id          = e.Id,
        Name        = e.Name,
        Category    = e.Category,
        PriceHalf   = e.PriceHalf,
        PriceFull   = e.PriceFull,
        Description = e.Description,
        IsAvailable = e.IsAvailable,
        ImageEmoji  = e.ImageEmoji,
    };

    // ── Cart (in-memory only) ─────────────────────────────────────────────────

    private List<CartItem> _cart = new();
    public List<CartItem> Cart => _cart;
    public int     CartCount => _cart.Sum(c => c.Quantity);
    public decimal CartTotal => _cart.Sum(c => c.Subtotal);

    public void AddToCart(FoodItem item, string portion)
    {
        var price    = portion == "Half" ? item.PriceHalf : item.PriceFull;
        var existing = _cart.FirstOrDefault(c => c.MenuItemId == item.Id && c.Portion == portion);
        if (existing is not null)
            existing.Quantity++;
        else
            _cart.Add(new CartItem
            {
                MenuItemId = item.Id,
                Name       = item.Name,
                Category   = item.Category,
                Portion    = portion,
                UnitPrice  = price,
                Quantity   = 1,
            });
        Notify();
    }

    public void UpdateCartQuantity(CartItem item, int delta)
    {
        item.Quantity += delta;
        if (item.Quantity <= 0) _cart.Remove(item);
        Notify();
    }

    public void RemoveFromCart(CartItem item) { _cart.Remove(item); Notify(); }

    public void ClearCart() { _cart.Clear(); Notify(); }

    // ── Cart snapshot for card payment navigation ─────────────────────────────

    private List<CartItem> _pendingCartSnapshot = new();

    public void SnapshotCartForPayment()
    {
        _pendingCartSnapshot = _cart.Select(c => new CartItem
        {
            MenuItemId = c.MenuItemId, Name = c.Name, Category = c.Category,
            Portion = c.Portion, UnitPrice = c.UnitPrice, Quantity = c.Quantity,
        }).ToList();
        ClearCart();
    }

    // ── Orders ────────────────────────────────────────────────────────────────

    private List<CustomerOrder> _orders = new();
    public  List<CustomerOrder> Orders  => _orders;

    public async Task RefreshOrdersAsync()
    {
        var headers = await _db.GetOrdersAsync();
        var result  = new List<CustomerOrder>();
        foreach (var h in headers)
        {
            var lines = await _db.GetOrderLinesAsync(h.Id);
            result.Add(new CustomerOrder
            {
                Id            = h.Id,
                OrderDate     = h.OrderDate,
                Status        = h.Status,
                PaymentMethod = h.PaymentMethod,
                PaymentStatus = h.PaymentStatus,
                IsPickedUp    = h.IsPickedUp,
                Items         = lines.Select(l => new CartItem
                {
                    MenuItemId = l.MenuItemId, Name = l.Name, Category = l.Category,
                    Portion = l.Portion, UnitPrice = l.UnitPrice, Quantity = l.Quantity,
                }).ToList(),
            });
        }
        _orders = result;
        Notify();
    }

    public CustomerOrder PlaceOrder(string paymentMethod)
    {
        var items = _cart.Select(c => new CartItem
        {
            MenuItemId=c.MenuItemId, Name=c.Name, Category=c.Category,
            Portion=c.Portion, UnitPrice=c.UnitPrice, Quantity=c.Quantity,
        }).ToList();
        ClearCart();
        return SaveAndSimulateOrder(items, paymentMethod, string.Empty);
    }

    public CustomerOrder PlaceOrderAfterPayment(string paymentMethod, string transactionId)
    {
        var items = _pendingCartSnapshot.Any() ? _pendingCartSnapshot : _cart.ToList();
        _pendingCartSnapshot.Clear();
        if (_cart.Any()) ClearCart();
        return SaveAndSimulateOrder(items, paymentMethod, transactionId);
    }

    private CustomerOrder SaveAndSimulateOrder(
        List<CartItem> items, string paymentMethod, string transactionId)
    {
        var order = new CustomerOrder
        {
            OrderDate     = DateTime.Now,
            Status        = string.IsNullOrEmpty(transactionId) ? "Pending" : "Confirmed",
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentMethod == "COP" ? "Pending" : "Paid",
            Items         = items,
        };

        // Fire-and-forget: persist to SQLite then simulate progress
        _ = PersistOrderAsync(order, transactionId);
        return order;
    }

    private async Task PersistOrderAsync(CustomerOrder order, string transactionId)
    {
        var header = new CustomerOrderEntity
        {
            OrderDate     = order.OrderDate,
            Status        = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            IsPickedUp    = false,
            TransactionId = transactionId,
        };
        var lines = order.Items.Select(i => new OrderLineEntity
        {
            MenuItemId = i.MenuItemId, Name = i.Name, Category = i.Category,
            Portion = i.Portion, UnitPrice = i.UnitPrice, Quantity = i.Quantity,
        }).ToList();

        await _db.InsertOrderWithLinesAsync(header, lines);
        order.Id = header.Id;

        if (!string.IsNullOrEmpty(transactionId))
        {
            await AddNotificationAsync(new CustomerNotification
            {
                Title     = "Payment Successful! 💳",
                Message   = $"Order #{order.Id} confirmed. Txn: {transactionId}",
                Type      = "PaymentConfirmed",
                CreatedAt = DateTime.Now,
            });
        }

        _orders.Insert(0, order);
        Notify();
        await SimulateOrderProgressAsync(order, header.Id);
    }

    private async Task SimulateOrderProgressAsync(CustomerOrder order, int dbId)
    {
        await Task.Delay(5000);
        order.Status = "Confirmed"; order.PaymentStatus = "Paid";
        await _db.UpdateOrderStatusAsync(dbId, "Confirmed", "Paid");
        await AddNotificationAsync(new CustomerNotification
        {
            Title="Order Confirmed! 🎉", Message=$"Order #{order.Id} has been confirmed.",
            Type="OrderConfirmed", CreatedAt=DateTime.Now,
        });
        Notify();

        await Task.Delay(10000);
        order.Status = "Preparing";
        await _db.UpdateOrderStatusAsync(dbId, "Preparing", "Paid");
        await AddNotificationAsync(new CustomerNotification
        {
            Title="Being Prepared 👨‍🍳", Message=$"Order #{order.Id} is now being prepared.",
            Type="General", CreatedAt=DateTime.Now,
        });
        Notify();

        await Task.Delay(15000);
        order.Status = "Ready for Pickup";
        await _db.UpdateOrderStatusAsync(dbId, "Ready for Pickup", "Paid");
        await AddNotificationAsync(new CustomerNotification
        {
            Title="Ready for Pick-Up! 🛍️", Message=$"Order #{order.Id} is ready at the counter.",
            Type="OrderReady", CreatedAt=DateTime.Now,
        });
        Notify();
    }

    // ── Notifications ─────────────────────────────────────────────────────────

    private List<CustomerNotification> _notifications = new();
    public  List<CustomerNotification> Notifications  => _notifications;
    public  int UnreadCount => _notifications.Count(n => !n.IsRead);

    public async Task RefreshNotificationsAsync()
    {
        var entities = await _db.GetNotificationsAsync();
        _notifications = entities.Select(e => new CustomerNotification
        {
            Id        = e.Id,
            Title     = e.Title,
            Message   = e.Message,
            CreatedAt = e.CreatedAt,
            IsRead    = e.IsRead,
            Type      = e.Type,
        }).ToList();
        Notify();
    }

    public async Task AddNotificationAsync(CustomerNotification notif)
    {
        var entity = new NotificationEntity
        {
            Title     = notif.Title,
            Message   = notif.Message,
            CreatedAt = notif.CreatedAt,
            IsRead    = notif.IsRead,
            Type      = notif.Type,
        };
        await _db.InsertNotificationAsync(entity);
        notif.Id = entity.Id;
        _notifications.Insert(0, notif);
        Notify();
    }

    // Keep sync wrapper for backward compat with existing code
    public void AddNotification(CustomerNotification notif)
        => _ = AddNotificationAsync(notif);

    public async Task MarkReadAsync(int id)
    {
        await _db.MarkNotificationReadAsync(id);
        var n = _notifications.FirstOrDefault(x => x.Id == id);
        if (n is not null) { n.IsRead = true; Notify(); }
    }

    public void MarkRead(int id) => _ = MarkReadAsync(id);

    public async Task MarkAllReadAsync()
    {
        await _db.MarkAllNotificationsReadAsync();
        _notifications.ForEach(n => n.IsRead = true);
        Notify();
    }

    public void MarkAllRead() => _ = MarkAllReadAsync();

    public async Task DeleteNotificationAsync(int id)
    {
        await _db.DeleteNotificationAsync(id);
        _notifications.RemoveAll(n => n.Id == id);
        Notify();
    }

    public void DeleteNotification(int id) => _ = DeleteNotificationAsync(id);
}
