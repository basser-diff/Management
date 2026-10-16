namespace Resto.Client.Models;

public class InventoryItem
{
    public int    Id         { get; set; }
    public int    ProductId  { get; set; }
    public string Name       { get; set; } = string.Empty;
    public string Category   { get; set; } = string.Empty;
    public int    Stock      { get; set; }
    public int    LowStockThreshold { get; set; } = 5;

    /// <summary>Derived status based on Stock level.</summary>
    public StockStatus Status => Stock <= 0
        ? StockStatus.OutOfStock
        : Stock <= LowStockThreshold
            ? StockStatus.AddStock
            : StockStatus.InStock;
}

public enum StockStatus
{
    InStock,
    AddStock,
    OutOfStock
}
