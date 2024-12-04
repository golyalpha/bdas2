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
        var rooms = Room.ListRooms(); // Získání seznamu místností
        return View(rooms);
    }

    // GET: Room/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // Zobrazí formuláø Create.cshtml
    }

    // POST: Room/Create
    [HttpPost]
    public IActionResult Create(Room room)
    {
        if (ModelState.IsValid)
        {
            // Logika pro pøidání nové místnosti
            _logger.LogInformation("New room created successfully.");
            return RedirectToAction("Index");
        }
        return View(room);
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
