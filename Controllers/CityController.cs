using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class CityController : Controller
{
    private readonly ILogger<CityController> _logger;

    public CityController(ILogger<CityController> logger)
    {
        _logger = logger;
    }

    // GET: City/Index
    public IActionResult Index()
    {
        var cities = City.ListCities();
        
        ViewBag.IsAdmin = User.IsInRole("Administrator");
        ViewBag.IsManager = User.IsInRole("Manager");
        
        return View(cities);
    }

    // GET: City/Create
    [HttpGet]
    [Authorize(Roles = "Administrator")]  // Pouze Admin může přidávat země/města
    public IActionResult Create()
    {
        ViewBag.Countries = Country.ListCountries();
        return View();
    }

    // POST: City/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public IActionResult Create(City city, int countryId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Countries = Country.ListCountries();
            return View(city);
        }

        try
        {
            city.Country = Country.GetCountry(countryId);
            city.Persist();
            
            TempData["Success"] = "Město bylo úspěšně vytvořeno.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při vytváření města");
            ModelState.AddModelError("", $"Chyba při vytváření města: {ex.Message}");
            ViewBag.Countries = Country.ListCountries();
            return View(city);
        }
    }

    // GET: City/CreateCountry
    [HttpGet]
    [Authorize(Roles = "Administrator")]  // Pouze Admin může přidávat země
    public IActionResult CreateCountry()
    {
        return View();
    }

    // POST: City/CreateCountry
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateCountry(Country country)
    {
        if (!ModelState.IsValid)
        {
            return View(country);
        }

        try
        {
            country.Persist();
            
            TempData["Success"] = "Země byla úspěšně vytvořena.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při vytváření země");
            ModelState.AddModelError("", $"Chyba při vytváření země: {ex.Message}");
            return View(country);
        }
    }
}