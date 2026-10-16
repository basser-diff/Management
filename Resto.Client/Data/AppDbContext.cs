using Microsoft.EntityFrameworkCore;
using Resto.Client.Models;

namespace Resto.Client.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Manager> Managers => Set<Manager>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // MANAGERS
        // ============================================================

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Phone)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        // ============================================================
        // PAYMENTS
        // ============================================================

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerNameEnc)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.TransactionRefEnc)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.Method)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();
        });

        // ============================================================
        // PRODUCTS
        // ============================================================

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.PriceHalf)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.PriceFull)
                .HasColumnType("decimal(18,2)");
        });

        // ============================================================
        // ORDER ITEMS
        // ============================================================

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)");
        });
    }
}