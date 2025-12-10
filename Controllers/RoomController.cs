using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;
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
        var rooms = WebApp.Models.Room.ListRooms();
        var isGuest = User.IsInRole("Guest");
        var isAdmin = User.IsInRole("Administrator");
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        ViewBag.IsGuest = isGuest;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.CurrentUserId = currentUserId;

        return View(rooms);
    }

    // GET: Room/Create
    [HttpGet]
    public IActionResult Create()
    {
        if (User.IsInRole("Guest"))
        {
            TempData["Error"] = "Uživatelé s rolí Guest nemohou vytvářet místnosti.";
            return RedirectToAction("Index");
        }

        var model = new RoomViewModel
        {
            Room = new Room  
            {
                Id = 0,
                Name = "",
                Capacity = 1,
                Type = "MEETING_ROOM",
                Location = new Location { Id = 0 },
                Organiser = new Organiser { Id = 0 }
            },
            Locations = Location.ListLocations(),
            Organisers = Organiser.ListOrganisers(),
            LocationId = 0,
            OrganiserId = 0
        };

        return View(model);
    }

    // POST: Room/Create
    [HttpPost]
    public IActionResult Create(RoomViewModel roomViewModel)
    {
        if (User.IsInRole("Guest"))
        {
            TempData["Error"] = "Uživatelé s rolí Guest nemohou vytvářet místnosti.";
            return RedirectToAction("Index");
        }

        // Validace modelu
        if (!ModelState.IsValid)
        {
            roomViewModel.Locations = Location.ListLocations();
            roomViewModel.Organisers = Organiser.ListOrganisers();
            return View(roomViewModel);
        }

        // SP1: Kapacita musí být kladná
        if (roomViewModel.Room.Capacity <= 0)
        {
            ModelState.AddModelError("Room.Capacity", 
                "Kapacita místnosti musí být kladná (SP1)");
            roomViewModel.Locations = Location.ListLocations();
            roomViewModel.Organisers = Organiser.ListOrganisers();
            return View(roomViewModel);
        }

        // SP3: Velikost pódia pro presentation room
        if (roomViewModel.Room.Type == "PRESENTATION_ROOM")
        {
            if (!roomViewModel.PodiumSize.HasValue || roomViewModel.PodiumSize.Value <= 0)
            {
                ModelState.AddModelError("PodiumSize", 
                    "Velikost pódia musí být kladná (SP3)");
                roomViewModel.Locations = Location.ListLocations();
                roomViewModel.Organisers = Organiser.ListOrganisers();
                return View(roomViewModel);
            }
        }

        try
        {
            var room = roomViewModel.Room;
            room.Location = Location.GetLocation(roomViewModel.LocationId);
            room.Organiser = Organiser.GetOrganiser(roomViewModel.OrganiserId);
            
            if (room.Type == "MEETING_ROOM")
            {
                var meetingRoom = new MeetingRoom
                {
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Type = room.Type,
                    Location = room.Location,
                    Organiser = room.Organiser,
                    VideoCallReady = roomViewModel.VideoCallReady ?? false
                };
                meetingRoom.Persist();
            }
            else if (room.Type == "PRESENTATION_ROOM")
            {
                var presentationRoom = new PresentationRoom
                {
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Type = room.Type,
                    Location = room.Location,
                    Organiser = room.Organiser,
                    PodiumSize = roomViewModel.PodiumSize.Value
                };
                presentationRoom.Persist();
            }
            
            TempData["Success"] = "Místnost byla úspěšně vytvořena";
            return RedirectToAction("Index");
        }
        catch (OracleException ex)
        {
            // Zpracování specifických DB chyb
            if (ex.Message.Contains("rooms_name_un"))
            {
                ModelState.AddModelError("Room.Name",
                    "Místnost s tímto názvem již existuje (IO7)");
            }
            else if (ex.Message.Contains("chk_room_capacity_positive"))
            {
                ModelState.AddModelError("Room.Capacity", "Kapacita musí být kladná (IO1)");
            }
            else if (ex.Message.Contains("chk_podium_size_positive"))
            {
                ModelState.AddModelError("PodiumSize", "Velikost pódia musí být kladná (IO3)");
            }
            else
            {
                ModelState.AddModelError("", 
                    "Nastala chyba při ukládání: " + ex.Message);
            }
            
            roomViewModel.Locations = Location.ListLocations();
            roomViewModel.Organisers = Organiser.ListOrganisers();
            return View(roomViewModel);
        }
    }

    // POST: Room/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (User.IsInRole("Guest"))
        {
            TempData["Error"] = "Uživatelé s rolí Guest nemohou editovat místnosti.";
            return RedirectToAction("Index");
        }

        var room = Room.GetRoom(id);
        if (room == null)
        {
            return NotFound();
        }
        var model = new RoomViewModel
        {
            Room = room,
            Locations = Location.ListLocations(),
            Organisers = Organiser.ListOrganisers(),
            VideoCallReady = room is MeetingRoom meetingRoom ? meetingRoom.VideoCallReady : (bool?)null,
            PodiumSize = room is PresentationRoom presentationRoom ? presentationRoom.PodiumSize : (int?)null
        };

        return View("Create", model); // Reuse Create view for editing
    }

    [HttpPost]
    public IActionResult Edit(RoomViewModel roomViewModel, int id)
    {
        if (User.IsInRole("Guest"))
        {
            TempData["Error"] = "Uživatelé s rolí Guest nemohou editovat místnosti.";
            return RedirectToAction("Index");
        }

        try
        {
            var room = roomViewModel.Room;
            room.Id = id;
            room.Location = Location.GetLocation(roomViewModel.LocationId);
            room.Organiser = Organiser.GetOrganiser(roomViewModel.OrganiserId);

            if (room.Type == "MEETING_ROOM")
            {
                var meetingRoom = new MeetingRoom
                {
                    Id = id,
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Type = room.Type,
                    Location = room.Location,
                    Organiser = room.Organiser,
                    VideoCallReady = roomViewModel.VideoCallReady ?? false
                };
                meetingRoom.Persist();
            }
            else if (room.Type == "PRESENTATION_ROOM")
            {
                var presentationRoom = new PresentationRoom
                {
                    Id = id,
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Type = room.Type,
                    Location = room.Location,
                    Organiser = room.Organiser,
                    PodiumSize = roomViewModel.PodiumSize ?? 0
                };
                presentationRoom.Persist();
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room.");
            ModelState.AddModelError("", "An error occurred while saving the room.");
            return View("Create", roomViewModel);
        }
    }

    // POST: Room/Delete
    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (User.IsInRole("Guest"))
        {
            _logger.LogWarning("Guest user attempted to delete room {RoomId}", id);
            TempData["Error"] = "Uživatelé s rolí Guest nemohou mazat místnosti.";
            return RedirectToAction("Index");
        }

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
