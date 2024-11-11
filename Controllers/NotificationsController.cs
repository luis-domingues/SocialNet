using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNet.Context;

namespace SocialNet.Controllers;

public class NotificationsController : Controller
{
    private readonly SocialNetDbContext _context;

    public NotificationsController(SocialNetDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirstValue("UserId"));
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ClearReadNotifications()
    {
        var userId = int.Parse(User.FindFirstValue("UserId"));
        var readNotifications = _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead);

        _context.Notifications.RemoveRange(readNotifications);
        await _context.SaveChangesAsync();

        return Ok();
    }
}