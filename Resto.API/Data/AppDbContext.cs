using Microsoft.EntityFrameworkCore;
using Resto.API.Models;

namespace Resto.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Manager> Managers => Set<Manager>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // MANAGER
        // =========================

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Phone)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired();
        });

        // =========================
        // PAYMENT
        // =========================

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerNameEnc)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.TransactionRefEnc)
                .HasMaxLength(1000);

            entity.Property(x => x.Method)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // ORDER
        // =========================

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.PaymentStatus)
                .IsRequired();
        });

        // =========================
        // ORDER ITEM
        // =========================

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.ProductName)
                .IsRequired();

            entity.Property(x => x.Portion)
                .IsRequired();

            entity.Property(x => x.Category)
                .IsRequired();
        });

        // =========================
        // PRODUCT
        // =========================

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Description)
                .IsRequired();

            entity.Property(x => x.Category)
                .IsRequired();

            entity.Property(x => x.PriceHalf)
                .HasPrecision(18, 2);

            entity.Property(x => x.PriceFull)
                .HasPrecision(18, 2);

            entity.Property(x => x.IsAvailable)
                .IsRequired();
        });

        // =========================
        // INVENTORY
        // =========================

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Category)
                .IsRequired();

            entity.Property(x => x.Stock)
                .IsRequired();

            entity.Property(x => x.LowStockThreshold)
                .IsRequired();
        });

        // =========================
        // NOTIFICATIONS
        // =========================

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired();

            entity.Property(x => x.Title)
                .IsRequired();

            entity.Property(x => x.Message)
                .IsRequired();
        });
    }
}


// =====================================================
// ENTITIES
// =====================================================

public class Manager
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}


public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerNameEnc { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? TransactionRefEnc { get; set; }

    public Order? Order { get; set; }
}


public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
}


public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Portion { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}


public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal PriceHalf { get; set; }

    public decimal PriceFull { get; set; }

    public bool IsAvailable { get; set; }
}


public class InventoryItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Stock { get; set; }

    public int LowStockThreshold { get; set; }
}


public enum NotificationType
{
    NewOrder = 0,
    PaymentConfirmed = 1,
    LowStock = 2,
    OrderReady = 3,
    General = 4
}


public class Notification
{
    public int Id { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}