using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class LocationController : Controller
{
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILogger<LocationController> logger)
    {
        _logger = logger;
    }

    // GET: Location/Index
    public IActionResult Index()
    {
        var locations = Location.ListLocations();
        
        // Přidáme informace o aktuálním uživateli pro UI
        ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        ViewBag.IsAdmin = User.IsInRole("Administrator");
        ViewBag.IsManager = User.IsInRole("Manager");
        
        return View(locations);
    }

    // GET: Location/Create
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Create()
    {
        var model = new LocationViewModel
        {
            Location = new Location
            {
                Id = 0,
                Name = "",
                AvailabilityStart = new TimeOnly(8, 0),
                AvailabilityEnd = new TimeOnly(17, 0)
            },
            CityId = 0,
            Cities = City.ListCities()
        };
        
        return View(model);
    }

    // POST: Location/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Create(LocationViewModel model)
    {
        // Validace času: Začátek musí být před koncem
        if (model.Location.AvailabilityStart >= model.Location.AvailabilityEnd)
        {
            ModelState.AddModelError("Location.AvailabilityEnd", 
                "Dostupnost do musí být později než dostupnost od.");
        }

        if (!ModelState.IsValid)
        {
            model.Cities = City.ListCities();
            return View(model);
        }

        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var organiser = Organiser.GetOrganiser(currentUserId);
            
            model.Location.City = City.GetCity(model.CityId);
            model.Location.Organiser = organiser;
            model.Location.Persist();
            
            TempData["Success"] = "Budova byla úspěšně vytvořena.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při vytváření budovy");
            ModelState.AddModelError("", $"Chyba při vytváření budovy: {ex.Message}");
            model.Cities = City.ListCities();
            return View(model);
        }
    }

    // GET: Location/Edit/5
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Edit(int id)
    {
        Location location;
        try
        {
            location = Location.GetLocation(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("Manager");

        // Kontrola oprávnění
        if (!isAdmin && isManager && location.Organiser.Id != currentUserId)
        {
            TempData["Error"] = "Můžete upravovat pouze vlastní budovy.";
            return RedirectToAction("Index");
        }

        var model = new LocationViewModel
        {
            Location = location,
            CityId = location.City.Id,
            Cities = City.ListCities()
        };

        return View("Create", model); // Používáme stejný formulář jako Create
    }

    // POST: Location/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Edit(int id, LocationViewModel model)
    {
        Location existingLocation;
        try
        {
            existingLocation = Location.GetLocation(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("Manager");

        // Kontrola oprávnění
        if (!isAdmin && isManager && existingLocation.Organiser.Id != currentUserId)
        {
            return Unauthorized();
        }

        // Validace času: Začátek musí být před koncem
        if (model.Location.AvailabilityStart >= model.Location.AvailabilityEnd)
        {
            ModelState.AddModelError("Location.AvailabilityEnd", 
                "Dostupnost do musí být později než dostupnost od.");
        }

        if (!ModelState.IsValid)
        {
            model.Cities = City.ListCities();
            return View("Create", model);
        }

        try
        {
            // Aktualizace existujících dat (zachováme původního organizátora)
            existingLocation.Name = model.Location.Name;
            existingLocation.AvailabilityStart = model.Location.AvailabilityStart;
            existingLocation.AvailabilityEnd = model.Location.AvailabilityEnd;
            existingLocation.City = City.GetCity(model.CityId);
            existingLocation.Persist();
            
            TempData["Success"] = "Budova byla úspěšně upravena.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při úpravě budovy");
            ModelState.AddModelError("", $"Chyba při úpravě budovy: {ex.Message}");
            model.Cities = City.ListCities();
            return View("Create", model);
        }
    }

    // POST: Location/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Delete(int id)
    {
        try
        {
            Location location = Location.GetLocation(id);
            
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            var isManager = User.IsInRole("Manager");

            // Kontrola oprávnění pro mazání
            if (!isAdmin && isManager && location.Organiser.Id != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete location {LocationId} owned by {OwnerId}", 
                    currentUserId, id, location.Organiser.Id);
                TempData["Error"] = "Můžete mazat pouze vlastní budovy.";
                return RedirectToAction("Index");
            }
            
            location.Delete();
            TempData["Success"] = "Budova byla úspěšně smazána.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při mazání budovy");
            TempData["Error"] = $"Chyba při mazání budovy: {ex.Message}";
        }
        
        return RedirectToAction("Index");
    }
}