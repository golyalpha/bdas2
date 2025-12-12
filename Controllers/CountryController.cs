using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public class CountryController : Controller
    {
        private readonly ILogger<CountryController> _logger;

        public CountryController(ILogger<CountryController> logger)
        {
            _logger = logger;
        }

        // GET: Country/Index
        public IActionResult Index()
        {
            var isAdmin = User.IsInRole("Administrator");
            var countries = Country.ListCountries();
            
            ViewBag.IsAdmin = isAdmin;
            return View(countries);
        }

        // GET: Country/Create
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Country { Name = "" });
        }

        // POST: Country/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Country model)
        {
            // Validace: Název nesmí být prázdný
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Název zemì je povinný");
                return View(model);
            }

            // Validace: Název musí mít alespoò 2 znaky
            if (model.Name.Trim().Length < 2)
            {
                ModelState.AddModelError("Name", "Název zemì musí mít alespoò 2 znaky");
                return View(model);
            }

            // Validace: Název mùže mít maximálnì 32 znakù (dle DB schématu)
            if (model.Name.Length > 32)
            {
                ModelState.AddModelError("Name", "Název zemì mùže mít maximálnì 32 znakù");
                return View(model);
            }

            // Validace: Kontrola duplicit (case-insensitive)
            var existingCountries = Country.ListCountries();
            if (existingCountries.Any(c => c.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "Zemì s tímto názvem již existuje");
                return View(model);
            }

            try
            {
                model.Name = model.Name.Trim(); // Oøíznutí bílých znakù
                model.Persist();
                
                _logger.LogInformation("Country created: {CountryName} by user {UserId}", 
                    model.Name, User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                TempData["Success"] = $"Zemì '{model.Name}' byla úspìšnì vytvoøena";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating country: {CountryName}", model.Name);
                ModelState.AddModelError("", $"Chyba pøi vytváøení zemì: {ex.Message}");
                return View(model);
            }
        }

        // POST: Country/Delete/{id}
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var country = Country.GetCountry(id);
                
                // Pokud má zemì mìsta, CASCADE je smaže automaticky (dle DB schématu)
                // DELETE FROM cities CASCADE
                
                _logger.LogWarning("Country deleted: ID={CountryId}, Name={CountryName} by user {UserId}", 
                    id, country.Name, User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                TempData["Success"] = $"Zemì '{country.Name}' byla úspìšnì smazána";
                country.Delete();
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Zemì nebyla nalezena";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting country: ID={CountryId}", id);
                TempData["Error"] = $"Chyba pøi mazání zemì: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }
    }
}