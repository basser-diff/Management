using SQLite;

namespace Resto.App.Models;

/// <summary>SQLite table for menu items.</summary>
[Table("FoodItems")]
public class FoodItemEntity
{
    [PrimaryKey, AutoIncrement]
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public decimal PriceHalf   { get; set; }
    public decimal PriceFull   { get; set; }
    public string  Description { get; set; } = string.Empty;
    public bool    IsAvailable { get; set; } = true;
    public string  ImageEmoji  { get; set; } = string.Empty;
}

/// <summary>SQLite table for customer orders (header).</summary>
[Table("Orders")]
public class CustomerOrderEntity
{
    [PrimaryKey, AutoIncrement]
    public int      Id            { get; set; }
    public DateTime OrderDate     { get; set; }
    public string   Status        { get; set; } = "Pending";
    public string   PaymentMethod { get; set; } = "COP";
    public string   PaymentStatus { get; set; } = "Pending";
    public bool     IsPickedUp    { get; set; }
    public string   TransactionId { get; set; } = string.Empty;
}

/// <summary>SQLite table for individual items within an order.</summary>
[Table("OrderLines")]
public class OrderLineEntity
{
    [PrimaryKey, AutoIncrement]
    public int     Id         { get; set; }
    public int     OrderId    { get; set; }
    public int     MenuItemId { get; set; }
    public string  Name       { get; set; } = string.Empty;
    public string  Category   { get; set; } = string.Empty;
    public string  Portion    { get; set; } = "Full";
    public int     Quantity   { get; set; }
    public decimal UnitPrice  { get; set; }
}

/// <summary>SQLite table for in-app notifications.</summary>
[Table("Notifications")]
public class NotificationEntity
{
    [PrimaryKey, AutoIncrement]
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Message   { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool     IsRead    { get; set; }
    public string   Type      { get; set; } = "General";
}
