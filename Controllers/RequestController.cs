using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
public class RequestController : Controller
{
    private readonly ILogger<RequestController> _logger;

    public RequestController(ILogger<RequestController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var requests = WebApp.Models.Request.ListRequests(); // Získání seznamu žádostí pro zobrazení v indexu
        return View(requests);
    }

    // GET: Request/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // Zobrazí formuláø Create.cshtml
    }

    // POST: Request/Create
    [HttpPost]
    public IActionResult Create(Request request)
    {
        if (ModelState.IsValid)
        {
            // Logika pro pøidání nové žádosti (napøíklad ukládání do databáze)
            _logger.LogInformation("New request created successfully.");
            return RedirectToAction("Index");
        }
        return View(request);
    }

    // GET: Request/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var request = WebApp.Models.Request.GetRequest(id); // Získání konkrétní žádosti podle ID
        if (request == null)
        {
            return NotFound();
        }
        return View(request); // Zobrazí formuláø Edit.cshtml s pøedvyplnìnými hodnotami
    }

    // POST: Request/Update
    [HttpPost]
    public IActionResult Update(Request updatedRequest)
    {
        if (ModelState.IsValid)
        {
            var existingRequest = (Request)WebApp.Models.Request.GetRequest(updatedRequest.Id);
            if (existingRequest != null)
            {
                // Aktualizace dat existující žádosti
                existingRequest.Start = updatedRequest.Start;
                existingRequest.End = updatedRequest.End;
                existingRequest.Organiser = updatedRequest.Organiser;

                _logger.LogInformation("Request updated successfully.");
                return RedirectToAction("Index");
            }
        }
        return View("Edit", updatedRequest); // V pøípadì chyby se vrátí na editovací stránku
    }
}
