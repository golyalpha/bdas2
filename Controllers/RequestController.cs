using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
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
        // Admin vidí všechny, ostatní jen své
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        
        var requests = isAdmin 
            ? RoomRequest.ListRequests() 
            : RoomRequest.GetRequestsByOrganiserId(currentUserId);
            
        ViewBag.IsAdmin = isAdmin;
        ViewBag.CurrentUserId = currentUserId;
        return View(requests);
    }


    // GET: Request/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new RoomRequestViewModel
        {
            Request = new RoomRequest(),
            MeetingRequest = new MeetingRequest { VideoCallReady = false }, // defaultní hodnota
            PresentationRequest = new PresentationRequest { PodiumSize = 0 }, // defaultní hodnota
            Locations = Location.ListLocations()
        };
        return View(model);
    }

    // POST: Request/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RoomRequestViewModel model)
    {
        // SP6: Musí být zadána buď délka nebo konec
        if (model.Request.End == default)
        {
            ModelState.AddModelError("Request.End", 
                "Musíte zadat konec rezervace (SP6)");
            model.Locations = Location.ListLocations();
            return View(model);
        }

        // SP5: Konec musí být po začátku
        if (model.Request.End != default && 
            model.Request.Start >= model.Request.End)
        {
            ModelState.AddModelError("Request.End", 
                "Konec rezervace musí být po začátku (SP5)");
            model.Locations = Location.ListLocations();
            return View(model);
        }

        // SP1: Minimální kapacita musí být kladná
        if (model.Request.MinimumCapacity <= 0)
        {
            ModelState.AddModelError("Request.MinimumCapacity", 
                "Minimální kapacita musí být kladná (SP1)");
            model.Locations = Location.ListLocations();
            return View(model);
        }

        try
        {
            var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organiser = Organiser.GetOrganiser(Int32.Parse(organiserIdString));
            
            RoomRequest request = null;
            
            // Vytvoření instance podle typu
            if (model.Request.Type == "MEETING_RREQUEST") 
            {
                request = new MeetingRequest 
                { 
                    VideoCallReady = model.MeetingRequest?.VideoCallReady ?? false 
                };
            }
            else if (model.Request.Type == "PRESENTATION_RREQUEST") 
            {
                var podiumSize = model.PresentationRequest?.PodiumSize ?? 0;
                
                // SP3: Validace velikosti pódia
                if (podiumSize <= 0)
                {
                    ModelState.AddModelError("PresentationRequest.PodiumSize", 
                        "Velikost pódia musí být kladná (SP3)");
                    model.Locations = Location.ListLocations();
                    return View(model);
                }
                
                request = new PresentationRequest 
                { 
                    PodiumSize = podiumSize 
                };
            }
            
            if (request == null)
            {
                ModelState.AddModelError("Request.Type", "Invalid Room Request Type");
                model.Locations = Location.ListLocations();
                return View(model);
            }
            
            // Nastavení všech vlastností
            request.Location = Location.GetLocation(model.LocationId);
            request.MinimumCapacity = model.Request.MinimumCapacity;
            request.Start = model.Request.Start;
            request.End = model.Request.End;
            request.Type = model.Request.Type;
            request.Organiser = organiser;
            
            request.Length = model.Request.End - model.Request.Start;
            
            _logger.LogInformation("Creating request with Length: {Length} (from {Start} to {End})", 
                request.Length, request.Start, request.End);
            
            // Uložení do databáze
            request.Persist();
            
            TempData["Success"] = "Žádost byla úspěšně vytvořena";
            return RedirectToAction("Index");
        }
        catch (OracleException ex)
        {
            _logger.LogError(ex, "Chyba při vytváření žádosti");
            
            // Zpracování DB chyb
            if (ex.Number == 20001)
            {
                ModelState.AddModelError("", 
                    "Požadavek musí obsahovat délku nebo konec (IO6)");
            }
            else if (ex.Number == 20002)
            {
                ModelState.AddModelError("Request.End", 
                    "Konec musí být po začátku (IO5)");
            }
            else if (ex.Number == 20003)
            {
                ModelState.AddModelError("Request.MinimumCapacity", 
                    "Kapacita musí být kladná (IO1)");
            }
            else
            {
                ModelState.AddModelError("", 
                    $"Nastala chyba: {ex.Message}");
            }
            
            model.Locations = Location.ListLocations();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při vytváření žádosti");
            ModelState.AddModelError("", $"Neočekávaná chyba: {ex.Message}");
            model.Locations = Location.ListLocations();
            return View(model);
        }
    }

    // GET: Request/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        RoomRequest request;
        try {
            request = RoomRequest.GetRequest(id);
        } catch (KeyNotFoundException) {
            return NotFound();
        }

        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        // Admin může editovat vše, ostatní jen své
        if (!isAdmin && request.Organiser.Id != int.Parse(organiserIdString))
        {
            return Unauthorized();
        }

        var model = new RoomRequestViewModel {
            Request = request,
            LocationId = request.Location.Id,
            Locations = Location.ListLocations()
        };
        if (request.Type == "MEETING_RREQUEST") {
            model.MeetingRequest = (MeetingRequest)request;
        }
        if (request.Type == "PRESENTATION_RREQUEST") {
            model.PresentationRequest = (PresentationRequest)request;
        }
        return View("Create", model); // Zobrazí formulář Edit.cshtml s předvyplněnými hodnotami
    }

    // POST: Request/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, RoomRequestViewModel model)
    {

        RoomRequest request;
        try {
            request = RoomRequest.GetRequest(id);
        } catch (KeyNotFoundException) {
            return NotFound();
        }

        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        // OPRAVA: Admin může editovat vše, ostatní jen své
        if (!isAdmin && request.Organiser.Id != int.Parse(organiserIdString))
        {
            return Unauthorized();
        }

        if (request.Type == "MEETING_RREQUEST") {
            ((MeetingRequest)request).VideoCallReady = model.MeetingRequest.VideoCallReady;
        }
        if (model.Request.Type == "PRESENTATION_RREQUEST") {
            ((PresentationRequest)request).PodiumSize = model.PresentationRequest.PodiumSize;
        }

        request.Location = Location.GetLocation(model.LocationId);
        request.MinimumCapacity = model.Request.MinimumCapacity;
        request.Start = model.Request.Start;
        request.End = model.Request.End;
        request.Length = model.Request.Length;
        request.Type = model.Request.Type;
        request.Persist();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            RoomRequest request = RoomRequest.GetRequest(id);
            
            // OPRAVA: Kontrola vlastnictví nebo admin práv
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            
            if (!isAdmin && request.Organiser.Id != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete request {RequestId} owned by {OwnerId}", 
                    currentUserId, id, request.Organiser.Id);
                return Unauthorized();
            }
            
            request.Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting request.");
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        RoomRequest request;
        try 
        {
            request = RoomRequest.GetRequest(id);
        } 
        catch (KeyNotFoundException) 
        {
            return NotFound();
        }

        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        
        if (!isAdmin && request.Organiser.Id != int.Parse(organiserIdString))
        {
            return Unauthorized();
        }

        // Nastavíme ViewBag pro indikaci read-only módu
        ViewBag.IsReadOnly = true;
        ViewBag.PageTitle = "Detail žádosti";
        
        return View("Edit", request);
    }
}
