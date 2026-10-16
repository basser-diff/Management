using System.ComponentModel.DataAnnotations;

namespace Resto.API.Models;

public class ProductResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal PriceHalf { get; set; }

    public decimal PriceFull { get; set; }

    public bool IsAvailable { get; set; }
}

public class CreateProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PriceHalf { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PriceFull { get; set; }

    public bool IsAvailable { get; set; } = true;
}

public class UpdateProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PriceHalf { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PriceFull { get; set; }

    public bool IsAvailable { get; set; }
}