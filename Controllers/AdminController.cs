using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Zobrazí hierarchickou strukturu organizátorù
    /// </summary>
    public IActionResult Hierarchy()
    {
        try
        {
            var hierarchy = OrganizerHierarchy.GetHierarchy();
            return View(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading organizer hierarchy");
            TempData["ErrorMessage"] = "Chyba pøi naèítání hierarchie organizátorù.";
            return RedirectToAction("Index", "User");
        }
    }
}