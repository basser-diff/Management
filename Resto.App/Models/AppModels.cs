namespace Resto.App.Models;

// ── Product / Menu ────────────────────────────────────────────────────────────
public class FoodItem
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public decimal PriceHalf   { get; set; }
    public decimal PriceFull   { get; set; }
    public string  Description { get; set; } = string.Empty;
    public bool    IsAvailable { get; set; } = true;
    public string? ImageEmoji  { get; set; }   // fallback visual
}

// ── Cart ─────────────────────────────────────────────────────────────────────
public class CartItem
{
    public int     MenuItemId  { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public string  Portion     { get; set; } = "Full";   // Half | Full
    public int     Quantity    { get; set; } = 1;
    public decimal UnitPrice   { get; set; }
    public decimal Subtotal    => UnitPrice * Quantity;
}

// ── Order ─────────────────────────────────────────────────────────────────────
public class CustomerOrder
{
    public int              Id           { get; set; }
    public DateTime         OrderDate    { get; set; }
    public string           Status       { get; set; } = "Pending";
    public string           PaymentMethod{ get; set; } = "COP";
    public string           PaymentStatus{ get; set; } = "Pending";
    public List<CartItem>   Items        { get; set; } = new();
    public decimal          Total        => Items.Sum(i => i.Subtotal);
    public bool             IsPickedUp   { get; set; }
}

// ── Notification ──────────────────────────────────────────────────────────────
public class CustomerNotification
{
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Message   { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool     IsRead    { get; set; }
    public string   Type      { get; set; } = "General";  // NewOrder|OrderReady|PaymentConfirmed
}
