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

    public IActionResult DatabaseObjects()
    {
        try
        {
            var objects = DatabaseObject.GetAllObjects();
            return View(objects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading database objects");
            TempData["ErrorMessage"] = "Chyba pøi naèítání databázových objektù.";
            return RedirectToAction("Hierarchy");
        }
    }

    public IActionResult AuditLog(string tableName = null)
    {
        try
        {
            var logs = WebApp.Models.AuditLog.GetRecentLogs(100, tableName);
            ViewBag.SelectedTable = tableName;
            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit log");
            TempData["ErrorMessage"] = "Chyba pøi naèítání audit logu.";
            return RedirectToAction("DatabaseObjects");
        }
    }
}