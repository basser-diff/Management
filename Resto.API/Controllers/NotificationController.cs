using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications =
            await _db.Notifications
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();

        return Ok(
            ApiResponse<List<Notification>>.Ok(
                notifications));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNotification(
        int id)
    {
        var notification =
            await _db.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (notification == null)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    "Notification not found."));
        }

        return Ok(
            ApiResponse<Notification>.Ok(
                notification));
    }
}