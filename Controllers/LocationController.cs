using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

[Authorize]
public class LocationController : Controller
{
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILogger<LocationController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var locations = WebApp.Models.Location.ListLocations();
        var isGuest = User.IsInRole("Guest");
        var isAdmin = User.IsInRole("Administrator");
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        ViewBag.IsGuest = isGuest;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.CurrentUserId = currentUserId;

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
            locationViewModel.Location.City = City.GetCity(locationViewModel.CityId);
            locationViewModel.Location.Persist();
            return RedirectToAction("Index");
    }

    // POST: Location/Create
    [HttpPost]
    public IActionResult Edit(LocationViewModel locationViewModel, int id)
    {
        if (User.IsInRole("Guest"))
        {
            TempData["Error"] = "Uživatelé s rolí Guest nemohou upravovat lokace.";
            return RedirectToAction("Index");
        }

        locationViewModel.Location.Id = id;
        System.Console.Out.WriteLine(locationViewModel.CityId);
        locationViewModel.Location.City = City.GetCity(locationViewModel.CityId);
        locationViewModel.Location.Persist();
        return RedirectToAction("Index");
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
        var model = new LocationViewModel
        {
            CityId = location.City.Id,
            Location = location
        };
        return View("Create", model); // Zobrazí formulář Edit.cshtml s předvyplněnými hodnotami
    }

    [HttpPost]
    public IActionResult Delete(int id) {
        var location = Location.GetLocation(id);
        if (location == null)
        {
            return NotFound();
        }
        else {
            location.Delete();
            return RedirectToAction("Index");
        }
    }
}