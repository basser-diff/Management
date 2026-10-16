using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrderController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Select(x => new OrderResponse
            {
                Id = x.Id,
                CustomerName = x.CustomerName,
                Status = x.Status,
                PaymentStatus = x.PaymentStatus
            })
            .ToListAsync();

        return Ok(
            ApiResponse<List<OrderResponse>>.Ok(
                orders));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new OrderResponse
            {
                Id = x.Id,
                CustomerName = x.CustomerName,
                Status = x.Status,
                PaymentStatus = x.PaymentStatus
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Order not found."));

        return Ok(
            ApiResponse<OrderResponse>.Ok(order));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        var order =
            await _db.Orders.FindAsync(id);

        if (order == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Order not found."));

        order.Status = request.Status;

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<object>.Ok(
                null!,
                "Order status updated."));
    }

    [HttpPut("{id:int}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(
        int id,
        [FromBody] UpdateOrderPaymentStatusRequest request)
    {
        var order =
            await _db.Orders.FindAsync(id);

        if (order == null)
            return NotFound(
                ApiResponse<object>.Fail(
                    "Order not found."));

        order.PaymentStatus = request.PaymentStatus;

        await _db.SaveChangesAsync();

        return Ok(
            ApiResponse<object>.Ok(
                null!,
                "Payment status updated."));
    }
}