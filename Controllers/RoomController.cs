using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

public class RoomController : Controller
{
    private readonly ILogger<RoomController> _logger;

    public RoomController(ILogger<RoomController> logger)
    {
        _logger = logger;
    }

    // Index: Zobrazí seznam místností
    public IActionResult Index()
    {
        var objects = WebApp.Models.Room.ListRooms();
        var rooms = new List<Room>();
        foreach (var obj in objects)
        {
            rooms.Add((Room)obj);
        }
        return View(rooms);
    }

    // GET: Room/Edit/{id}
    [HttpGet]
    [Route("Room/Edit/{id?}")]
    [Route("Room/Create")]
    public IActionResult Persist(int? id)
    {
        if (id == null || id == 0)
        {
            // Nový záznam
            var newRoom = new Room
            {
                Id = 0,
                Name = string.Empty,
                Capacity = 0,
                Type = "MEETING_ROOM", // Výchozí typ
                Location = new Location { Id = 0 },
                Organiser = new Organiser { Id = 0 }
            };
            return View("Persist", newRoom); // Naète formuláø Persist.cshtml
        }
        else
        {
            // Naèítáme existující záznam
            var room = Room.GetRoom(id.Value);
            if (room == null)
            {
                return NotFound();
            }
            return View("Persist", room); // Naète formuláø Persist.cshtml
        }
    }

    // POST: Room/Persist
    [HttpPost]
    public IActionResult Persist(IFormCollection form)
    {
        try
        {
            // Naètení spoleèných vlastností
            int id = int.TryParse(form["Id"], out var parsedId) ? parsedId : 0;
            string name = form["Name"];
            int capacity = int.Parse(form["Capacity"]);
            string type = form["Type"];
            int locationId = int.Parse(form["Location.Id"]);
            int organiserId = int.Parse(form["Organiser.Id"]);

            // Výbìr podtypu a volání Persist
            if (type == "MEETING_ROOM")
            {
                string vcReady = form["VideoCallReady"];
                var meetingRoom = new MeetingRoom
                {
                    Id = id,
                    Name = name,
                    Capacity = capacity,
                    Type = type,
                    Location = new Location { Id = locationId },
                    Organiser = new Organiser { Id = organiserId },
                    VideoCallReady = vcReady == "Y"
                };
                meetingRoom.Persist();
            }
            else if (type == "PRESENTATION_ROOM")
            {
                int podiumSize = int.Parse(form["PodiumSize"]);
                var presentationRoom = new PresentationRoom
                {
                    Id = id,
                    Name = name,
                    Capacity = capacity,
                    Type = type,
                    Location = new Location { Id = locationId },
                    Organiser = new Organiser { Id = organiserId },
                    PodiumSize = podiumSize
                };
                presentationRoom.Persist();
            }
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving room.");
            return RedirectToAction("Index");
        }
    }

    // POST: Room/Delete
    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            Room room = Room.GetRoom(id);
            room.Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room.");
        }
        return RedirectToAction("Index");
    }
}
