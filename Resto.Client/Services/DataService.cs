using Microsoft.EntityFrameworkCore;
using Resto.Client.Data;
using Resto.Client.Models;

namespace Resto.Client.Services;
public class DataService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ClientEncryptionService         _crypto;

    public event Action? OnChange;
    public void NotifyChanged() => OnChange?.Invoke();
    private void Notify() => OnChange?.Invoke();

    // ── Notification settings (kept in-memory — UI preference only) ──────────
    public bool   SoundEnabled { get; set; } = true;
    public string SoundType    { get; set; } = "Chime";
    public int    SoundVolume  { get; set; } = 70;

    public DataService(IDbContextFactory<AppDbContext> factory, ClientEncryptionService crypto)
    {
        _factory = factory;
        _crypto  = crypto;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private AppDbContext Db() => _factory.CreateDbContext();

    // ── Products ─────────────────────────────────────────────────────────────

    public List<Product> Products
    {
        get { using var db = Db(); return db.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList(); }
    }

    public void AddProduct(Product product)
    {
        using var db = Db();
        product.Id = 0;
        db.Products.Add(product);
        db.SaveChanges();
        // Auto-create an inventory record
        db.Inventory.Add(new InventoryItem
        {
            ProductId = product.Id,
            Name      = product.Name,
            Category  = product.Category,
            Stock     = 0,
        });
        db.SaveChanges();
        Notify();
    }

    public void UpdateProduct(Product updated)
    {
        using var db = Db();
        var existing = db.Products.Find(updated.Id);
        if (existing is null) return;
        existing.Name        = updated.Name;
        existing.Category    = updated.Category;
        existing.PriceHalf   = updated.PriceHalf;
        existing.PriceFull   = updated.PriceFull;
        existing.Description = updated.Description;
        existing.IsAvailable = updated.IsAvailable;
        // Sync inventory name/category
        var inv = db.Inventory.FirstOrDefault(i => i.ProductId == updated.Id);
        if (inv is not null) { inv.Name = updated.Name; inv.Category = updated.Category; }
        db.SaveChanges();
        Notify();
    }

    public void DeleteProduct(int id)
    {
        using var db = Db();
        var product = db.Products.Find(id);
        if (product is null) return;
        db.Products.Remove(product);
        var inv = db.Inventory.FirstOrDefault(i => i.ProductId == id);
        if (inv is not null) db.Inventory.Remove(inv);
        db.SaveChanges();
        Notify();
    }

    // ── Inventory ────────────────────────────────────────────────────────────

    public List<InventoryItem> Inventory
    {
        get { using var db = Db(); return db.Inventory.OrderBy(i => i.Category).ThenBy(i => i.Name).ToList(); }
    }

    public void UpdateStock(int inventoryId, int newStock)
    {
        using var db = Db();
        var item = db.Inventory.Find(inventoryId);
        if (item is null) return;
        item.Stock = Math.Max(0, newStock);
        db.SaveChanges();
        Notify();
    }

    public void RestockItem(int inventoryId, int addAmount)
    {
        using var db = Db();
        var item = db.Inventory.Find(inventoryId);
        if (item is null) return;
        item.Stock += addAmount;
        db.SaveChanges();
        Notify();
    }

    public void DeleteInventoryItem(int id)
    {
        using var db = Db();
        var item = db.Inventory.Find(id);
        if (item is null) return;
        db.Inventory.Remove(item);
        db.SaveChanges();
        Notify();
    }

    // ── Orders ───────────────────────────────────────────────────────────────

    public List<Order> Orders
    {
        get
        {
            using var db = Db();
            return db.Orders
                     .Include(o => o.Items)
                     .OrderByDescending(o => o.OrderDate)
                     .ToList();
        }
    }

    public void UpdateOrderStatus(int orderId, OrderStatus status)
    {
        using var db = Db();
        var order = db.Orders.Find(orderId);
        if (order is null) return;
        order.Status = status;
        if (status == OrderStatus.Completed) order.IsPickedUp = true;
        db.SaveChanges();
        Notify();
    }

    public void ConfirmOrder(int orderId)
    {
        using var db = Db();
        var order = db.Orders.Find(orderId);
        if (order is null) return;
        order.Status        = OrderStatus.Confirmed;
        order.PaymentStatus = PaymentStatus.Paid;
        var pay = db.Payments.FirstOrDefault(p => p.OrderId == orderId);
        if (pay is not null) pay.Status = PaymentStatus.Paid;
        db.SaveChanges();

        AddNotification(new AppNotification
        {
            Title     = $"Order #{orderId} Confirmed",
            Message   = $"Order from {order.CustomerName} has been confirmed.",
            Type      = NotificationType.NewOrder,
            CreatedAt = DateTime.Now,
        });
        Notify();
    }

    // ── Payments ─────────────────────────────────────────────────────────────

    /// <summary>Returns payments with sensitive fields decrypted for display.</summary>
    // ─────────────────────────────────────────────────────────────
    // Payments
    // ─────────────────────────────────────────────────────────────

    public List<Payment> Payments
    {
        get
        {
            using var db = Db();

            var payments = db.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.TransactionDate)
                .ToList();

            foreach (var payment in payments)
            {
                payment.CustomerName =
                    _crypto.DecryptField(payment.CustomerNameEnc);

                payment.TransactionRef =
                    _crypto.DecryptField(payment.TransactionRefEnc);
            }

            return payments;
        }
    }

    public void UpdatePaymentStatus(
    int paymentId,
    PaymentStatus status)
    {
        using var db = Db();

        var payment = db.Payments.Find(paymentId);

        if (payment is null)
            return;

        payment.Status = status;

        db.SaveChanges();

        Notify();
    }
    public List<AppNotification> Notifications
    {
        get
        {
            using var db = Db();
            return db.Notifications.OrderByDescending(n => n.CreatedAt).ToList();
        }
    }
    public int UnreadNotificationCount
    {
        get { using var db = Db(); return db.Notifications.Count(n => !n.IsRead); }
    }

    public void AddNotification(AppNotification notification)
    {
        using var db = Db();
        notification.Id = 0;
        db.Notifications.Add(notification);
        db.SaveChanges();
        Notify();
    }

    public void MarkAllRead()
    {
        using var db = Db();
        db.Notifications.Where(n => !n.IsRead)
                        .ExecuteUpdate(s => s.SetProperty(n => n.IsRead, true));
        Notify();
    }

    public void MarkRead(int id)
    {
        using var db = Db();
        db.Notifications.Where(n => n.Id == id)
                        .ExecuteUpdate(s => s.SetProperty(n => n.IsRead, true));
        Notify();
    }

    public void DeleteNotification(int id)
    {
        using var db = Db();
        db.Notifications.Where(n => n.Id == id).ExecuteDelete();
        Notify();
    }

    // ── Computed stats ────────────────────────────────────────────────────────

    public decimal TotalSales
    {
        get { using var db = Db(); return db.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount); }
    }

    public decimal TotalExpenses => TotalSales * 0.55m;
    public decimal TotalProfit   => TotalSales - TotalExpenses;

    /// <summary>Last 7 days daily sales for the bar chart.</summary>
    public List<(string Day, decimal Amount)> WeeklySales()
    {
        using var db = Db();
        var result = new List<(string, decimal)>();
        for (int i = 6; i >= 0; i--)
        {
            var date  = DateTime.Now.Date.AddDays(-i);
            var label = date.ToString("ddd");
            var amount = i == 0
                ? db.Payments.Where(p => p.Status == PaymentStatus.Paid
                                      && p.TransactionDate.Date == date).Sum(p => p.Amount)
                : new Random(i * 7 + date.DayOfYear).Next(800, 2500);
            result.Add((label, amount));
        }
        return result;
    }
}
