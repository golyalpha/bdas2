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
        request.Location = Location.GetLocation(model.LocationId);
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
        RoomRequest request;
        try {
            request = RoomRequest.GetRequest(id);
        } catch (KeyNotFoundException) {
            return NotFound();
        }

        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (request.Organiser.Id != int.Parse(organiserIdString)){
            return Unauthorized();
        }

        var model = new RoomRequestViewModel {
            Request = request,
            LocationId = request.Location.Id

        };
        if (request.Type == "MEETING_RREQUEST") {
            model.MeetingRequest = (MeetingRequest)request;
        }
        if (request.Type == "PRESENTATION_RREQUEST") {
            model.PresentationRequest = (PresentationRequest)request;
        }
        return View("Create", model); // Zobraz� formul�� Edit.cshtml s p�edvypln�n�mi hodnotami
    }

    // POST: Request/Edit/5
    [HttpPost]
    public IActionResult Edit(int id, RoomRequestViewModel model)
    {

        RoomRequest request;
        try {
            request = RoomRequest.GetRequest(id);
        } catch (KeyNotFoundException) {
            return NotFound();
        }

        var organiserIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (request.Organiser.Id != int.Parse(organiserIdString)){
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
