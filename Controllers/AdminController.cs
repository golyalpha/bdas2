using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using WebApp.Models;
using WebApp.Util;

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

    // AUDIT LOG - použijte tento
    public IActionResult AuditLog(
        int page = 1,
        int pageSize = 50,
        string? tableName = null,
        string? operation = null,
        string? organizerName = null,  
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        try
        {
            _logger.LogInformation("=== AUDIT LOG DEBUG ===");
            _logger.LogInformation("Page: {Page}, PageSize: {PageSize}", page, pageSize);
            _logger.LogInformation("TableName: {TableName}, Operation: {Operation}", tableName ?? "NULL", operation ?? "NULL");
            
            var result = WebApp.Models.AuditLog.GetPagedAuditLogs(
                pageNumber: page,
                pageSize: pageSize,
                tableName: tableName,
                operation: operation,
                dateFrom: dateFrom,
                dateTo: dateTo
            );

            _logger.LogInformation("Result - Items count: {Count}, Total: {Total}, Pages: {Pages}", 
                result.Items.Count, result.TotalCount, result.TotalPages);

            ViewBag.CurrentPage = page;
            ViewBag.TableName = tableName;
            ViewBag.Operation = operation;
            ViewBag.OrganizerName = organizerName;  
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.PageSize = pageSize;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba pøi naèítání audit logu");
            TempData["Error"] = "Chyba pøi naèítání audit logu: " + ex.Message;
            return View(new PagedResult<AuditLog>());
        }
    }
}