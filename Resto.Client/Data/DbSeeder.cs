using Resto.Client.Models;
using Resto.Client.Services;

namespace Resto.Client.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
    AppDbContext db,
    ClientEncryptionService crypto)
    {
        await SeedManagerAsync(db, crypto);

        await SeedProductsAsync(db);

        await SeedInventoryAsync(db);

        await SeedOrdersAsync(db);

        await SeedPaymentsAsync(db, crypto);

        await SeedNotificationsAsync(db);
    }

    // ============================================================
    // MANAGER
    // ============================================================

    private static async Task SeedManagerAsync(
    AppDbContext db,
    ClientEncryptionService crypto)
    {
        if (db.Managers.Any())
            return;

        var passwordHash =
            crypto.HashPassword("Admin@123");

        db.Managers.Add(new Manager
        {
            FullName = "Admin Manager",

            Email = "admin@sarilinating.com",

            PasswordHash = passwordHash,

            Phone = "+63 912 345 6789",

            Role = "Manager"
        });

        await db.SaveChangesAsync();
    }

    // ============================================================
    // PRODUCTS
    // ============================================================

    private static async Task SeedProductsAsync(
        AppDbContext db)
    {
        if (db.Products.Any())
        {
            return;
        }

        db.Products.AddRange(
            new Product
            {
                Name = "Pork Afritada",
                Category = "Pork",
                PriceHalf = 55,
                PriceFull = 100,
                Description =
                    "Pork stewed in tomato sauce with potatoes and carrots.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Crispy Pata",
                Category = "Pork",
                PriceHalf = 120,
                PriceFull = 220,
                Description =
                    "Deep-fried pork leg served with vinegar dip.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Lechon Kawali",
                Category = "Pork",
                PriceHalf = 80,
                PriceFull = 150,
                Description =
                    "Pan-fried crispy pork belly.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Chicken Adobo",
                Category = "Chicken",
                PriceHalf = 60,
                PriceFull = 110,
                Description =
                    "Chicken braised in vinegar, soy sauce, and garlic.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Chicken Tinola",
                Category = "Chicken",
                PriceHalf = 55,
                PriceFull = 100,
                Description =
                    "Ginger-based chicken soup with papaya and malunggay.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Bistek Tagalog",
                Category = "Beef",
                PriceHalf = 75,
                PriceFull = 140,
                Description =
                    "Beef steak with onion rings in soy-calamansi sauce.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Beef Caldereta",
                Category = "Beef",
                PriceHalf = 80,
                PriceFull = 150,
                Description =
                    "Rich beef stew in tomato and liver sauce.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Pancit Bihon",
                Category = "Noodles",
                PriceHalf = 45,
                PriceFull = 85,
                Description =
                    "Stir-fried rice noodles with vegetables and meat.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Pancit Canton",
                Category = "Noodles",
                PriceHalf = 45,
                PriceFull = 85,
                Description =
                    "Egg noodles with mixed vegetables and soy sauce.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Seafood Kare-Kare",
                Category = "Seafood",
                PriceHalf = 90,
                PriceFull = 170,
                Description =
                    "Peanut-based stew with mixed seafood.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Sinigang na Hipon",
                Category = "Seafood",
                PriceHalf = 85,
                PriceFull = 160,
                Description =
                    "Sour tamarind broth with shrimp and vegetables.",
                IsAvailable = false
            },

            new Product
            {
                Name = "Pinakbet",
                Category = "Veggies",
                PriceHalf = 40,
                PriceFull = 75,
                Description =
                    "Mixed vegetables in shrimp paste.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Laing",
                Category = "Veggies",
                PriceHalf = 40,
                PriceFull = 75,
                Description =
                    "Taro leaves in coconut milk.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Steamed Rice",
                Category = "Rice",
                PriceHalf = 0,
                PriceFull = 15,
                Description =
                    "Plain steamed white rice.",
                IsAvailable = true
            },

            new Product
            {
                Name = "Garlic Fried Rice",
                Category = "Rice",
                PriceHalf = 0,
                PriceFull = 25,
                Description =
                    "Fried rice tossed with garlic.",
                IsAvailable = true
            }
        );

        await db.SaveChangesAsync();
    }

    // ============================================================
    // INVENTORY
    // ============================================================

    private static async Task SeedInventoryAsync(
        AppDbContext db)
    {
        if (db.Inventory.Any())
        {
            return;
        }

        var products =
            db.Products.ToList();

        int Id(string name)
        {
            return products
                .First(p => p.Name == name)
                .Id;
        }

        db.Inventory.AddRange(
            new InventoryItem
            {
                ProductId = Id("Pork Afritada"),
                Name = "Pork Afritada",
                Category = "Pork",
                Stock = 30,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Crispy Pata"),
                Name = "Crispy Pata",
                Category = "Pork",
                Stock = 12,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Lechon Kawali"),
                Name = "Lechon Kawali",
                Category = "Pork",
                Stock = 18,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Chicken Adobo"),
                Name = "Chicken Adobo",
                Category = "Chicken",
                Stock = 25,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Chicken Tinola"),
                Name = "Chicken Tinola",
                Category = "Chicken",
                Stock = 3,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Bistek Tagalog"),
                Name = "Bistek Tagalog",
                Category = "Beef",
                Stock = 14,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Beef Caldereta"),
                Name = "Beef Caldereta",
                Category = "Beef",
                Stock = 0,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Pancit Bihon"),
                Name = "Pancit Bihon",
                Category = "Noodles",
                Stock = 20,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Pancit Canton"),
                Name = "Pancit Canton",
                Category = "Noodles",
                Stock = 20,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Seafood Kare-Kare"),
                Name = "Seafood Kare-Kare",
                Category = "Seafood",
                Stock = 8,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Sinigang na Hipon"),
                Name = "Sinigang na Hipon",
                Category = "Seafood",
                Stock = 0,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Pinakbet"),
                Name = "Pinakbet",
                Category = "Veggies",
                Stock = 22,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Laing"),
                Name = "Laing",
                Category = "Veggies",
                Stock = 15,
                LowStockThreshold = 5
            },

            new InventoryItem
            {
                ProductId = Id("Steamed Rice"),
                Name = "Steamed Rice",
                Category = "Rice",
                Stock = 100,
                LowStockThreshold = 20
            },

            new InventoryItem
            {
                ProductId = Id("Garlic Fried Rice"),
                Name = "Garlic Fried Rice",
                Category = "Rice",
                Stock = 60,
                LowStockThreshold = 10
            }
        );

        await db.SaveChangesAsync();
    }

    // ============================================================
    // ORDERS
    // ============================================================

    private static async Task SeedOrdersAsync(
        AppDbContext db)
    {
        if (db.Orders.Any())
        {
            return;
        }

        var now = DateTime.Now;

        var orders = new List<Order>
        {
            new()
            {
                CustomerName = "Maria Santos",
                OrderDate = now.AddHours(-5),
                Status = OrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = true,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Pork Afritada",
                        Category = "Pork",
                        Portion = "Full",
                        Quantity = 2,
                        UnitPrice = 100
                    },

                    new()
                    {
                        ProductName = "Steamed Rice",
                        Category = "Rice",
                        Portion = "Full",
                        Quantity = 2,
                        UnitPrice = 15
                    }
                }
            },

            new()
            {
                CustomerName = "Juan dela Cruz",
                OrderDate = now.AddHours(-4),
                Status = OrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = true,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Crispy Pata",
                        Category = "Pork",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 220
                    },

                    new()
                    {
                        ProductName = "Pancit Bihon",
                        Category = "Noodles",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 85
                    }
                }
            },

            new()
            {
                CustomerName = "Ana Reyes",
                OrderDate = now.AddHours(-3),
                Status = OrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = true,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Chicken Adobo",
                        Category = "Chicken",
                        Portion = "Half",
                        Quantity = 2,
                        UnitPrice = 60
                    },

                    new()
                    {
                        ProductName = "Garlic Fried Rice",
                        Category = "Rice",
                        Portion = "Full",
                        Quantity = 2,
                        UnitPrice = 25
                    }
                }
            },

            new()
            {
                CustomerName = "Pedro Lim",
                OrderDate = now.AddHours(-2),
                Status = OrderStatus.ReadyForPickup,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Bistek Tagalog",
                        Category = "Beef",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 140
                    },

                    new()
                    {
                        ProductName = "Pancit Canton",
                        Category = "Noodles",
                        Portion = "Half",
                        Quantity = 1,
                        UnitPrice = 45
                    }
                }
            },

            new()
            {
                CustomerName = "Rosa Garcia",
                OrderDate = now.AddHours(-1.5),
                Status = OrderStatus.Preparing,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Seafood Kare-Kare",
                        Category = "Seafood",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 170
                    },

                    new()
                    {
                        ProductName = "Pinakbet",
                        Category = "Veggies",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 75
                    }
                }
            },

            new()
            {
                CustomerName = "Carlo Mendoza",
                OrderDate = now.AddHours(-1),
                Status = OrderStatus.Confirmed,
                PaymentStatus = PaymentStatus.Paid,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Lechon Kawali",
                        Category = "Pork",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 150
                    },

                    new()
                    {
                        ProductName = "Laing",
                        Category = "Veggies",
                        Portion = "Half",
                        Quantity = 1,
                        UnitPrice = 40
                    },

                    new()
                    {
                        ProductName = "Steamed Rice",
                        Category = "Rice",
                        Portion = "Full",
                        Quantity = 2,
                        UnitPrice = 15
                    }
                }
            },

            new()
            {
                CustomerName = "Leni Torres",
                OrderDate = now.AddMinutes(-45),
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Beef Caldereta",
                        Category = "Beef",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 150
                    },

                    new()
                    {
                        ProductName = "Steamed Rice",
                        Category = "Rice",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 15
                    }
                }
            },

            new()
            {
                CustomerName = "Ben Ocampo",
                OrderDate = now.AddMinutes(-30),
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Chicken Tinola",
                        Category = "Chicken",
                        Portion = "Full",
                        Quantity = 2,
                        UnitPrice = 100
                    }
                }
            },

            new()
            {
                CustomerName = "Tessie Aquino",
                OrderDate = now.AddMinutes(-15),
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Failed,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Pork Afritada",
                        Category = "Pork",
                        Portion = "Half",
                        Quantity = 3,
                        UnitPrice = 55
                    },

                    new()
                    {
                        ProductName = "Garlic Fried Rice",
                        Category = "Rice",
                        Portion = "Full",
                        Quantity = 3,
                        UnitPrice = 25
                    }
                }
            },

            new()
            {
                CustomerName = "Jun Villanueva",
                OrderDate = now.AddMinutes(-5),
                Status = OrderStatus.Cancelled,
                PaymentStatus = PaymentStatus.Failed,
                IsPickedUp = false,

                Items = new()
                {
                    new()
                    {
                        ProductName = "Crispy Pata",
                        Category = "Pork",
                        Portion = "Full",
                        Quantity = 1,
                        UnitPrice = 220
                    }
                }
            }
        };

        db.Orders.AddRange(orders);

        await db.SaveChangesAsync();
    }

    // ============================================================
    // PAYMENTS
    // ============================================================

    private static async Task SeedPaymentsAsync(
    AppDbContext db,
    ClientEncryptionService crypto)
    {
        if (db.Payments.Any())
            return;

        var orders = db.Orders
            .OrderBy(o => o.OrderDate)
            .ToList();

        if (orders.Count < 10)
            return;

        var now = DateTime.Now;

        db.Payments.AddRange(
            new Payment
            {
                OrderId = orders[0].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Maria Santos"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-001"),
                Amount = 230,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-5)
            },

            new Payment
            {
                OrderId = orders[1].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Juan dela Cruz"),
                Method = "COP",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-002"),
                Amount = 305,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-4)
            },

            new Payment
            {
                OrderId = orders[2].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Ana Reyes"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-003"),
                Amount = 170,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-3)
            },

            new Payment
            {
                OrderId = orders[3].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Pedro Lim"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-004"),
                Amount = 185,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-2)
            },

            new Payment
            {
                OrderId = orders[4].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Rosa Garcia"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-005"),
                Amount = 245,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-1.5)
            },

            new Payment
            {
                OrderId = orders[5].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Carlo Mendoza"),
                Method = "COP",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-006"),
                Amount = 220,
                Status = PaymentStatus.Paid,
                TransactionDate =
                    now.AddHours(-1)
            },

            new Payment
            {
                OrderId = orders[6].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Leni Torres"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-007"),
                Amount = 165,
                Status = PaymentStatus.Pending,
                TransactionDate =
                    now.AddMinutes(-45)
            },

            new Payment
            {
                OrderId = orders[7].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Ben Ocampo"),
                Method = "COP",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-008"),
                Amount = 200,
                Status = PaymentStatus.Pending,
                TransactionDate =
                    now.AddMinutes(-30)
            },

            new Payment
            {
                OrderId = orders[8].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Tessie Aquino"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-009"),
                Amount = 240,
                Status = PaymentStatus.Failed,
                TransactionDate =
                    now.AddMinutes(-15)
            },

            new Payment
            {
                OrderId = orders[9].Id,
                CustomerNameEnc =
                    crypto.EncryptField("Jun Villanueva"),
                Method = "Card",
                TransactionRefEnc =
                    crypto.EncryptField("TXN-010"),
                Amount = 220,
                Status = PaymentStatus.Failed,
                TransactionDate =
                    now.AddMinutes(-5)
            }
        );

        await db.SaveChangesAsync();
    }

    // ============================================================
    // NOTIFICATIONS
    // ============================================================

    private static async Task SeedNotificationsAsync(
        AppDbContext db)
    {
        if (db.Notifications.Any())
        {
            return;
        }

        var now = DateTime.Now;

        db.Notifications.AddRange(

            new AppNotification
            {
                Title = "New Order",
                Message =
                    "Ben Ocampo placed an order for Chicken Tinola.",
                CreatedAt = now.AddMinutes(-30),
                IsRead = false,
                Type = NotificationType.NewOrder
            },

            new AppNotification
            {
                Title = "New Order",
                Message =
                    "Leni Torres placed an order for Beef Caldereta.",
                CreatedAt = now.AddMinutes(-45),
                IsRead = false,
                Type = NotificationType.NewOrder
            },

            new AppNotification
            {
                Title = "Payment Confirmed",
                Message =
                    "Payment for Carlo Mendoza's order confirmed via COP.",
                CreatedAt = now.AddHours(-1),
                IsRead = false,
                Type = NotificationType.PaymentConfirmed
            },

            new AppNotification
            {
                Title = "Low Stock Alert",
                Message =
                    "Chicken Tinola stock is critically low (3 remaining).",
                CreatedAt = now.AddHours(-2),
                IsRead = true,
                Type = NotificationType.LowStock
            },

            new AppNotification
            {
                Title = "Order Ready",
                Message =
                    "Pedro Lim's order is ready for pick-up.",
                CreatedAt = now.AddHours(-2),
                IsRead = true,
                Type = NotificationType.OrderReady
            },

            new AppNotification
            {
                Title = "Out of Stock",
                Message =
                    "Beef Caldereta is now out of stock.",
                CreatedAt = now.AddHours(-3),
                IsRead = true,
                Type = NotificationType.LowStock
            }
        );

        await db.SaveChangesAsync();
    }
}