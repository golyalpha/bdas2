using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
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

    // GET: Room/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // Zobrazí formuláø Create.cshtml
    }

    [HttpPost]
    public IActionResult Create(IFormCollection form)
    {
        try
        {
            // Naèti spoleèné atributy
            string name = form["Name"];
            int capacity = int.Parse(form["Capacity"]);
            string type = form["Type"];
            int locationId = int.Parse(form["Location.Id"]);
            int organiserId = int.Parse(form["Organiser.Id"]);

            if (type == "MEETING_ROOM")
            {
                string vcReady = form["VideoCallReady"];
                var meetingRoom = new MeetingRoom
                {
                    Name = name,
                    Capacity = capacity,
                    Type = type,
                    Location = new Location { Id = locationId },
                    Organiser = new Organiser { Id = organiserId },
                    VideoCallReady = (vcReady == "Y")
                };
                meetingRoom.Create();
            }
            else if (type == "PRESENTATION_ROOM")
            {
                int podiumSize = int.TryParse(form["PodiumSize"], out var size) ? size : 0;
                var presentationRoom = new PresentationRoom
                {
                    Name = name,
                    Capacity = capacity,
                    Type = type,
                    Location = new Location { Id = locationId },
                    Organiser = new Organiser { Id = organiserId },
                    PodiumSize = podiumSize
                };
                presentationRoom.Create();
            }
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
            return View();
        }
    }

    // GET: Room/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var room = Room.GetRoom(id); // Získání konkrétní místnosti podle ID
        if (room == null)
        {
            return NotFound();
        }
        return View(room); // Zobrazí formuláø Edit.cshtml s pøedvyplnìnými hodnotami
    }

    // POST: Room/Update
    [HttpPost]
    public IActionResult Update(Room updatedRoom)
    {
        if (ModelState.IsValid)
        {
            var existingRoom = Room.GetRoom(updatedRoom.Id);
            if (existingRoom != null)
            {
                // Aktualizace dat existující místnosti
                existingRoom.Name = updatedRoom.Name;
                existingRoom.Capacity = updatedRoom.Capacity;
                existingRoom.Type = updatedRoom.Type;
                existingRoom.Location = updatedRoom.Location;

                _logger.LogInformation("Room updated successfully.");
                return RedirectToAction("Index");
            }
        }
        return View("Edit", updatedRoom); // V pøípadì chyby se vrátí na editovací stránku
    }
}
