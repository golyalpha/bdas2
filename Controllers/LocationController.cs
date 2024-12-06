using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

public class LocationController : Controller
{
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILogger<LocationController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var locations  = WebApp.Models.Location.ListLocations(); // Získání seznamu žádostí pro zobrazení v indexu
        return View(locations);
    }

    // GET: Location/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Location/Create
    [HttpPost]
    public IActionResult Create(Location location)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation("New location created successfully.");
            return RedirectToAction("Index");
        }
        return View(location);
    }

    // GET: Location/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var location = Location.GetLocation(id); // Získání konkrétní lokace podle ID
        if (location == null)
        {
            return NotFound();
        }
        return View(location); // Zobrazí formuláø Edit.cshtml s pøedvyplnìnými hodnotami
    }

    // POST: Location/Update
    [HttpPost]
    public IActionResult Update(Location updatedLocation)
    {
        if (ModelState.IsValid)
        {
            var existingLocation = Location.GetLocation(updatedLocation.Id);
            if (existingLocation != null)
            {
                // Aktualizace dat existující lokace
                existingLocation.Name = updatedLocation.Name;
                existingLocation.AvailabilityStart = updatedLocation.AvailabilityStart;
                existingLocation.AvailabilityEnd = updatedLocation.AvailabilityEnd;

                _logger.LogInformation("Location updated successfully.");
                return RedirectToAction("Index");
            }
        }
        return View("Edit", updatedLocation); // V pøípadì chyby se vrátí na editovací stránku
    }
}