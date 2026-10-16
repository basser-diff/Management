namespace Resto.Client.Models;

public class Order
{
    public int            Id              { get; set; }
    public string         CustomerName    { get; set; } = string.Empty;
    public DateTime       OrderDate       { get; set; }
    public OrderStatus    Status          { get; set; } = OrderStatus.Pending;
    public PaymentStatus  PaymentStatus   { get; set; } = PaymentStatus.Pending;
    public bool           IsPickedUp      { get; set; }
    public List<OrderItem> Items          { get; set; } = new();

    public decimal Total => Items.Sum(i => i.Subtotal);
}

public class OrderItem
{
    public int     Id        { get; set; }
    public int     OrderId   { get; set; }
    public int     ProductId { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public string  Category  { get; set; } = string.Empty;
    public string  Portion   { get; set; } = "Full";   // Half | Full
    public int     Quantity  { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal  => UnitPrice * Quantity;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    ReadyForPickup,
    Completed,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed
}
