namespace Resto.Client.Models;

public class Product
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public decimal PriceHalf   { get; set; }
    public decimal PriceFull   { get; set; }
    public string  Description { get; set; } = string.Empty;
    public bool    IsAvailable { get; set; } = true;
    public string? ImageUrl    { get; set; }
}
