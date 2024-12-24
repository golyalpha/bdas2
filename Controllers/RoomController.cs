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
        var rooms = WebApp.Models.Room.ListRooms();
        return View(rooms);
    }

    // GET: Room/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new RoomViewModel
        {
            Locations = Location.ListLocations(),
            Organisers = Organiser.ListOrganisers()
        };

        return View(model);
    }

    // POST: Room/Create
    [HttpPost]
    public IActionResult Create(RoomViewModel roomViewModel)
    {
        var room = roomViewModel.Room;
        room.Location = Location.GetLocation(roomViewModel.LocationId);
        room.Organiser = Organiser.GetOrganiser(roomViewModel.OrganiserId);
        if(room.Type == "MEETING_ROOM")
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
                PodiumSize = roomViewModel.PodiumSize ?? 0
            };
            presentationRoom.Persist();
        }
        return RedirectToAction("Index");
    }

    // POST: Room/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
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
