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
        var locations = WebApp.Models.Location.ListLocations(); // Získání seznamu žádostí pro zobrazení v indexu
        foreach (var item in locations)
        {
            System.Console.Out.WriteLine(item);
        }
        return View(locations);
    }

    // GET: Location/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new LocationViewModel());
    }

    // POST: Location/Create
    [HttpPost]
    public IActionResult Create(LocationViewModel locationViewModel)
    {
        if (ModelState.IsValid)
        {
            locationViewModel.Location.City = City.GetCity(locationViewModel.CityId);
            locationViewModel.Location.Persist();
            _logger.LogInformation("New location created successfully.");
            return RedirectToAction("Index");
        }
        return View(locationViewModel);
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