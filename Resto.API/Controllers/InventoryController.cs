using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public InventoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory()
    {
        var inventory =
            await _db.Inventory
                .AsNoTracking()
                .ToListAsync();

        return Ok(
            ApiResponse<List<InventoryItem>>.Ok(
                inventory));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var inventory =
            await _db.Inventory
                .AsNoTracking()
                .Where(x =>
                    x.Stock <= x.LowStockThreshold)
                .ToListAsync();

        return Ok(
            ApiResponse<List<InventoryItem>>.Ok(
                inventory));
    }

    [HttpPut("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(
        int id,
        [FromBody] int stock)
    {
        if (stock < 0)
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Stock cannot be negative."));
        }

        var item =
            await _db.Inventory.FindAsync(id);

        if (item == null)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    "Inventory item not found."));
        }

        item.Stock = stock;

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<object>.Ok(
                null!,
                "Stock updated successfully."));
    }
}