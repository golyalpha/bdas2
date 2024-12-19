using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            System.Console.Out.WriteLine(locationViewModel.CityId);
            locationViewModel.Location.City = City.GetCity(locationViewModel.CityId);
            locationViewModel.Location.Persist();
            return RedirectToAction("Index");
    }

    // POST: Location/Create
    [HttpPost]
    public IActionResult Edit(LocationViewModel locationViewModel, int id)
    {
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
        return View("Create", model); // Zobrazí formuláø Edit.cshtml s pøedvyplnìnými hodnotami
    }

}