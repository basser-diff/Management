using SQLite;
using Resto.App.Models;

namespace Resto.App.Data;

/// <summary>
/// SQLite local database for the RESTO customer mobile app.
/// Stores: FoodItems (menu), CustomerOrders, CartItems, CustomerNotifications.
/// The database file lives in the app's local data directory on each device.
/// </summary>
public class AppDatabase
{
    private readonly SQLiteAsyncConnection _db;

    public AppDatabase(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite
            | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    }

    // ── Initialise — create tables and seed on first run ─────────────────────

    public async Task InitialiseAsync()
    {
        await _db.CreateTableAsync<FoodItemEntity>();
        await _db.CreateTableAsync<CustomerOrderEntity>();
        await _db.CreateTableAsync<OrderLineEntity>();
        await _db.CreateTableAsync<NotificationEntity>();

        await SeedMenuAsync();
    }

    // ── FoodItem (Menu) ───────────────────────────────────────────────────────

    public Task<List<FoodItemEntity>> GetMenuAsync()
        => _db.Table<FoodItemEntity>().OrderBy(f => f.Category).ToListAsync();

    public Task<List<FoodItemEntity>> GetMenuByCategoryAsync(string category)
        => _db.Table<FoodItemEntity>().Where(f => f.Category == category).ToListAsync();

    public Task<List<FoodItemEntity>> SearchMenuAsync(string query)
        => _db.Table<FoodItemEntity>()
              .Where(f => f.Name.Contains(query) || f.Category.Contains(query))
              .ToListAsync();

    public Task<FoodItemEntity?> GetFoodItemAsync(int id)
        => _db.Table<FoodItemEntity>().Where(f => f.Id == id).FirstOrDefaultAsync();

    // ── Orders ────────────────────────────────────────────────────────────────

    public async Task<int> SaveOrderAsync(CustomerOrderEntity order)
    {
        if (order.Id == 0)
            return await _db.InsertAsync(order);
        await _db.UpdateAsync(order);
        return order.Id;
    }

    public async Task SaveOrderLinesAsync(IEnumerable<OrderLineEntity> lines)
    {
        await _db.DeleteAllAsync<OrderLineEntity>(); // replace strategy for demo
        foreach (var line in lines)
            if (line.Id == 0) await _db.InsertAsync(line);
            else              await _db.UpdateAsync(line);
    }

    public async Task InsertOrderWithLinesAsync(CustomerOrderEntity order, List<OrderLineEntity> lines)
    {
        await _db.InsertAsync(order);
        foreach (var line in lines)
        {
            line.OrderId = order.Id;
            await _db.InsertAsync(line);
        }
    }

    public Task<List<CustomerOrderEntity>> GetOrdersAsync()
        => _db.Table<CustomerOrderEntity>().OrderByDescending(o => o.OrderDate).ToListAsync();

    public Task<List<OrderLineEntity>> GetOrderLinesAsync(int orderId)
        => _db.Table<OrderLineEntity>().Where(l => l.OrderId == orderId).ToListAsync();

    public async Task UpdateOrderStatusAsync(int orderId, string status, string paymentStatus)
    {
        var order = await _db.Table<CustomerOrderEntity>().Where(o => o.Id == orderId).FirstOrDefaultAsync();
        if (order is null) return;
        order.Status        = status;
        order.PaymentStatus = paymentStatus;
        await _db.UpdateAsync(order);
    }

    // ── Notifications ─────────────────────────────────────────────────────────

    public Task<List<NotificationEntity>> GetNotificationsAsync()
        => _db.Table<NotificationEntity>().OrderByDescending(n => n.CreatedAt).ToListAsync();

    public Task<int> InsertNotificationAsync(NotificationEntity notif)
        => _db.InsertAsync(notif);

    public async Task MarkNotificationReadAsync(int id)
    {
        var n = await _db.Table<NotificationEntity>().Where(x => x.Id == id).FirstOrDefaultAsync();
        if (n is null) return;
        n.IsRead = true;
        await _db.UpdateAsync(n);
    }

    public async Task MarkAllNotificationsReadAsync()
    {
        var all = await _db.Table<NotificationEntity>().Where(n => !n.IsRead).ToListAsync();
        foreach (var n in all) { n.IsRead = true; await _db.UpdateAsync(n); }
    }

    public Task DeleteNotificationAsync(int id)
        => _db.Table<NotificationEntity>().DeleteAsync(n => n.Id == id);

    public Task<int> GetUnreadCountAsync()
        => _db.Table<NotificationEntity>().CountAsync(n => !n.IsRead);

    // ── Seed — insert menu on first run ───────────────────────────────────────

    private async Task SeedMenuAsync()
    {
        if (await _db.Table<FoodItemEntity>().CountAsync() > 0) return;

        var menu = new List<FoodItemEntity>
        {
            new(){Name="Pork Afritada",    Category="Pork",    PriceHalf=55,  PriceFull=100, Description="Pork stewed in tomato sauce with potatoes and carrots.", IsAvailable=true,  ImageEmoji="🥘"},
            new(){Name="Crispy Pata",       Category="Pork",    PriceHalf=120, PriceFull=220, Description="Deep-fried pork leg served with vinegar dip.",           IsAvailable=true,  ImageEmoji="🍖"},
            new(){Name="Lechon Kawali",     Category="Pork",    PriceHalf=80,  PriceFull=150, Description="Pan-fried crispy pork belly.",                           IsAvailable=true,  ImageEmoji="🥩"},
            new(){Name="Chicken Adobo",     Category="Chicken", PriceHalf=60,  PriceFull=110, Description="Chicken braised in vinegar, soy sauce, and garlic.",     IsAvailable=true,  ImageEmoji="🍗"},
            new(){Name="Chicken Tinola",    Category="Chicken", PriceHalf=55,  PriceFull=100, Description="Ginger-based chicken soup with papaya and malunggay.",   IsAvailable=true,  ImageEmoji="🍲"},
            new(){Name="Bistek Tagalog",    Category="Beef",    PriceHalf=75,  PriceFull=140, Description="Beef steak with onion rings in soy-calamansi sauce.",    IsAvailable=true,  ImageEmoji="🥩"},
            new(){Name="Beef Caldereta",    Category="Beef",    PriceHalf=80,  PriceFull=150, Description="Rich beef stew in tomato and liver sauce.",              IsAvailable=false, ImageEmoji="🍛"},
            new(){Name="Pancit Bihon",      Category="Noodles", PriceHalf=45,  PriceFull=85,  Description="Stir-fried rice noodles with vegetables and meat.",     IsAvailable=true,  ImageEmoji="🍜"},
            new(){Name="Pancit Canton",     Category="Noodles", PriceHalf=45,  PriceFull=85,  Description="Egg noodles with mixed vegetables.",                    IsAvailable=true,  ImageEmoji="🍝"},
            new(){Name="Seafood Kare-Kare", Category="Seafood", PriceHalf=90,  PriceFull=170, Description="Peanut-based stew with mixed seafood.",                 IsAvailable=true,  ImageEmoji="🦐"},
            new(){Name="Sinigang na Hipon", Category="Seafood", PriceHalf=85,  PriceFull=160, Description="Sour tamarind broth with shrimp and vegetables.",       IsAvailable=false, ImageEmoji="🍤"},
            new(){Name="Pinakbet",          Category="Veggies", PriceHalf=40,  PriceFull=75,  Description="Mixed vegetables in shrimp paste.",                    IsAvailable=true,  ImageEmoji="🥦"},
            new(){Name="Laing",             Category="Veggies", PriceHalf=40,  PriceFull=75,  Description="Taro leaves in coconut milk.",                         IsAvailable=true,  ImageEmoji="🌿"},
            new(){Name="Steamed Rice",      Category="Rice",    PriceHalf=0,   PriceFull=15,  Description="Plain steamed white rice.",                            IsAvailable=true,  ImageEmoji="🍚"},
            new(){Name="Garlic Fried Rice", Category="Rice",    PriceHalf=0,   PriceFull=25,  Description="Fried rice tossed with garlic.",                       IsAvailable=true,  ImageEmoji="🍳"},
        };
        await _db.InsertAllAsync(menu);

        // Seed welcome notification
        await _db.InsertAsync(new NotificationEntity
        {
            Title     = "Welcome to RESTO! 👋",
            Message   = "Browse our menu and place your first order.",
            Type      = "General",
            CreatedAt = DateTime.Now,
            IsRead    = false,
        });
    }
}
