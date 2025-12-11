using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class RoomController : Controller
{
    private readonly ILogger<RoomController> _logger;

    public RoomController(ILogger<RoomController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var rooms = Room.ListRooms();
        
        ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        ViewBag.IsAdmin = User.IsInRole("Administrator");
        ViewBag.IsManager = User.IsInRole("Manager");
        
        return View(rooms);
    }

    // GET: Room/Create
    [HttpGet]
    [Authorize(Roles = "Administrator,Manager")]  // Pouze Admin a Manager
    public IActionResult Create()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        
        // Manager vidí pouze vlastní budovy
        List<Location> locations;
        if (isAdmin)
        {
            locations = Location.ListLocations();
        }
        else
        {
            locations = Location.ListLocations()
                .Where(l => l.Organiser.Id == currentUserId)
                .ToList();
        }

        ViewBag.Locations = locations;
        ViewBag.IsAdmin = isAdmin;
        
        if (!locations.Any() && !isAdmin)
        {
            TempData["Warning"] = "Nemáte žádné vlastní budovy. Nejprve vytvořte budovu.";
        }
        
        return View();
    }

    // POST: Room/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Create(Room room, int locationId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        
        Location location;
        try
        {
            location = Location.GetLocation(locationId);
        }
        catch (KeyNotFoundException)
        {
            ModelState.AddModelError("", "Vybraná budova neexistuje.");
            return View(room);
        }

        // Manager může vytvářet místnosti pouze ve vlastních budovách
        if (!isAdmin && location.Organiser.Id != currentUserId)
        {
            TempData["Error"] = "Můžete vytvářet místnosti pouze ve vlastních budovách.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }

        try
        {
            var organiser = Organiser.GetOrganiser(currentUserId);
            
            room.Location = location;
            room.Organiser = organiser;
            
            // Persist se volá na konkrétní typ místnosti
            if (room.Type == "MEETING_ROOM")
            {
                var meetingRoom = room as MeetingRoom;
                meetingRoom?.Persist();
            }
            else if (room.Type == "PRESENTATION_ROOM")
            {
                var presentationRoom = room as PresentationRoom;
                presentationRoom?.Persist();
            }
            
            TempData["Success"] = "Místnost byla úspěšně vytvořena.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při vytváření místnosti");
            ModelState.AddModelError("", $"Chyba při vytváření místnosti: {ex.Message}");
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }
    }

    // GET: Room/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        Room room;
        try
        {
            room = Room.GetRoom(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("Manager");

        // Kontrola oprávnění:
        // - Admin může editovat vše
        // - Manager může editovat pouze místnosti ve vlastních budovách
        // - User a Guest nemohou editovat
        if (!isAdmin && !isManager)
        {
            TempData["Error"] = "Nemáte oprávnění upravovat místnosti.";
            return RedirectToAction("Index");
        }

        if (isManager && !isAdmin && room.Location.Organiser.Id != currentUserId)
        {
            TempData["Error"] = "Můžete upravovat pouze místnosti ve vlastních budovách.";
            return RedirectToAction("Index");
        }

        LoadLocationsForUser(currentUserId, isAdmin);
        ViewBag.IsAdmin = isAdmin;
        return View(room);
    }

    // POST: Room/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Room room, int locationId)
    {
        Room existingRoom;
        try
        {
            existingRoom = Room.GetRoom(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("Manager");

        // Kontrola oprávnění (stejná jako u GET)
        if (!isAdmin && !isManager)
        {
            return Unauthorized();
        }

        if (isManager && !isAdmin && existingRoom.Location.Organiser.Id != currentUserId)
        {
            return Unauthorized();
        }

        Location location;
        try
        {
            location = Location.GetLocation(locationId);
        }
        catch (KeyNotFoundException)
        {
            ModelState.AddModelError("", "Vybraná budova neexistuje.");
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }

        // Manager nemůže přesunout místnost do cizí budovy
        if (!isAdmin && location.Organiser.Id != currentUserId)
        {
            TempData["Error"] = "Nemůžete přesunout místnost do cizí budovy.";
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }

        if (!ModelState.IsValid)
        {
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }

        try
        {
            existingRoom.Name = room.Name;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Location = location;
            
            // Aktualizace specifických vlastností podle typu
            if (existingRoom is MeetingRoom meetingRoom && room is MeetingRoom newMeetingRoom)
            {
                meetingRoom.VideoCallReady = newMeetingRoom.VideoCallReady;
                meetingRoom.Persist();
            }
            else if (existingRoom is PresentationRoom presentationRoom && room is PresentationRoom newPresentationRoom)
            {
                presentationRoom.PodiumSize = newPresentationRoom.PodiumSize;
                presentationRoom.Persist();
            }
            
            TempData["Success"] = "Místnost byla úspěšně upravena.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při úpravě místnosti");
            ModelState.AddModelError("", $"Chyba při úpravě místnosti: {ex.Message}");
            LoadLocationsForUser(currentUserId, isAdmin);
            return View(room);
        }
    }

    // POST: Room/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,Manager")]
    public IActionResult Delete(int id)
    {
        try
        {
            Room room = Room.GetRoom(id);
            
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            var isManager = User.IsInRole("Manager");

            // Kontrola oprávnění pro mazání
            if (isManager && !isAdmin && room.Location.Organiser.Id != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete room {RoomId} in location owned by {OwnerId}", 
                    currentUserId, id, room.Location.Organiser.Id);
                TempData["Error"] = "Můžete mazat pouze místnosti ve vlastních budovách.";
                return RedirectToAction("Index");
            }
            
            room.Delete();
            TempData["Success"] = "Místnost byla úspěšně smazána.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při mazání místnosti");
            TempData["Error"] = $"Chyba při mazání místnosti: {ex.Message}";
        }
        
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Pomocná metoda pro načtení budov podle oprávnění uživatele
    /// </summary>
    private void LoadLocationsForUser(int userId, bool isAdmin)
    {
        if (isAdmin)
        {
            ViewBag.Locations = Location.ListLocations();
        }
        else
        {
            ViewBag.Locations = Location.ListLocations()
                .Where(l => l.Organiser.Id == userId)
                .ToList();
        }
    }
}
