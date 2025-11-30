using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Vrací poèet nepøeètených notifikací (používá PL/SQL funkci)
    /// </summary>
    [HttpGet]
    public IActionResult GetUnreadCount()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var organiserId))
        {
            _logger.LogWarning("Unauthorized access to GetUnreadCount - user not authenticated");
            return Unauthorized();
        }

        try
        {
            _logger.LogInformation($"Getting unread count for organizer {organiserId}");
            var count = Notification.GetUnreadCount(organiserId);
            _logger.LogInformation($"Organizer {organiserId} has {count} unread notifications");
            return Json(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unread notification count for organizer {organiserId}");
            return StatusCode(500, new { error = "Internal server error", count = 0 });
        }
    }

    [HttpGet]
    public IActionResult GetForCurrentUser()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var organiserId))
        {
            _logger.LogWarning("Unauthorized access to GetForCurrentUser");
            return Unauthorized();
        }

        try
        {
            var notifications = Notification.ListByOrganiserId(organiserId);
            
            var dto = notifications.Select(n => new
            {
                n.Id,
                n.NotificationType,
                Delivered = n.Delivered,
                RequestId = n.Request?.Id,
                LocationName = n.Request?.Location?.Name,
                RequestStart = n.Request?.Start
            }).ToList();

            _logger.LogInformation($"Loaded {dto.Count} notifications for organizer {organiserId}");
            return Json(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading notifications for organizer {organiserId}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult MarkDelivered([FromBody] int[] ids)
    {
        var token = Request.Headers["RequestVerificationToken"].FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Missing antiforgery token");
            return BadRequest("Missing antiforgery token");
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var organiserId))
        {
            _logger.LogWarning("Unauthorized - invalid organizer ID");
            return Unauthorized();
        }

        if (ids == null || ids.Length == 0)
        {
            _logger.LogWarning("Empty notification IDs array");
            return BadRequest("No notification IDs provided");
        }

        try
        {
            _logger.LogInformation($"MarkDelivered: organiserId={organiserId}, ids=[{string.Join(",", ids)}]");
            Notification.MarkAsDelivered(organiserId, ids);
            _logger.LogInformation("Notifications marked as delivered successfully");
            return Ok(new { success = true, message = "Notifications marked as delivered" });
        }
        catch (ApplicationException aex)
        {
            _logger.LogError(aex, "Application error marking notifications as delivered");
            return StatusCode(500, new { error = aex.Message, innerError = aex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error marking notifications as delivered");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var organiserId))
        {
            return Unauthorized();
        }

        var notifications = Notification.ListByOrganiserId(organiserId);
        ViewBag.UnreadCount = Notification.GetUnreadCount(organiserId);
        
        return View(notifications);
    }
}