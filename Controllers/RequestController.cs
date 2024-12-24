using System.Security.Claims;
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
        var requests = RoomRequest.ListRequests(); // Z�sk�n� seznamu ��dost� pro zobrazen� v indexu
        return View(requests);
    }


    // GET: Request/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new RoomRequestViewModel
        {
            Locations = Location.ListLocations()
        };
        return View(model);
    }

    // POST: Request/Create
    [HttpPost]
    public IActionResult Create(RoomRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organiser = Organiser.GetOrganiser(Int32.Parse(organiserIdString));
        RoomRequest request = null;
        if (model.Request.Type == "MEETING_RREQUEST") {
            request = model.MeetingRequest;
        }
        if (model.Request.Type == "PRESENTATION_RREQUEST") {
            request = model.PresentationRequest; 
        }
        if (request == null){
            ModelState.AddModelError(model.Request.Type, "Invalid Room Request Type");
            return View(model);
        }
        request.Location = model.Request.Location;
        request.MinimumCapacity = model.Request.MinimumCapacity;
        request.Start = model.Request.Start;
        request.End = model.Request.End;
        request.Length = model.Request.Length;
        request.Organiser = organiser;
        request.Type = model.Request.Type;
        request.Persist();
        return RedirectToAction("Index");
    }

    // GET: Request/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var request = RoomRequest.GetRequest(id); // Z�sk�n� konkr�tn� ��dosti podle ID
        if (request == null)
        {
            return NotFound();
        }
        return View(request); // Zobraz� formul�� Edit.cshtml s p�edvypln�n�mi hodnotami
    }

    // POST: Request/Update
    [HttpPost]
    public IActionResult Update(RoomRequest updatedRequest)
    {
        if (ModelState.IsValid)
        {
            var existingRequest = RoomRequest.GetRequest(updatedRequest.Id);
            if (existingRequest != null)
            {
                // Aktualizace dat existuj�c� ��dosti
                existingRequest.Start = updatedRequest.Start;
                existingRequest.End = updatedRequest.End;
                existingRequest.Organiser = updatedRequest.Organiser;

                _logger.LogInformation("Request updated successfully.");
                return RedirectToAction("Index");
            }
        }
        return View("Edit", updatedRequest); // V p��pad� chyby se vr�t� na editovac� str�nku
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            RoomRequest request = RoomRequest.GetRequest(id);
            request.Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room.");
        }
        return RedirectToAction("Index");
    }
}
