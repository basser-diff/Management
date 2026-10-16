using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Select(x => new ProductResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Category = x.Category,
                PriceHalf = x.PriceHalf,
                PriceFull = x.PriceFull,
                IsAvailable = x.IsAvailable
            })
            .ToListAsync();

        return Ok(
            ApiResponse<List<ProductResponse>>.Ok(
                products));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Category = x.Category,
                PriceHalf = x.PriceHalf,
                PriceFull = x.PriceFull,
                IsAvailable = x.IsAvailable
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Product not found."));

        return Ok(
            ApiResponse<ProductResponse>.Ok(product));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            PriceHalf = request.PriceHalf,
            PriceFull = request.PriceFull,
            IsAvailable = request.IsAvailable
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<int>.Ok(
                product.Id,
                "Product created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request)
    {
        var product =
            await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Product not found."));

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.PriceHalf = request.PriceHalf;
        product.PriceFull = request.PriceFull;
        product.IsAvailable = request.IsAvailable;

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<object>.Ok(
                null!,
                "Product updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product =
            await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Product not found."));

        _db.Products.Remove(product);

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<object>.Ok(
                null!,
                "Product deleted successfully."));
    }
}