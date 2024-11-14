using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

public class RoomController : Controller
{
    private readonly ILogger<RoomController> _logger;

    public RoomController(ILogger<RoomController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var rooms = new List<Room>
            {
                new Room{
                    Id = 1,
                    Name = "Room 1",
                    Capacity = 10,
                    Type = "Meeting",
                    Location = new Location
                    {
                        Id = 1,
                        Name = "Building 1",
                        AvailabilityStart = DateTime.Now.AddHours(-1),
                        AvailabilityEnd = DateTime.Now,
                    }
                },
                new Room{
                    Id = 2,
                    Name = "Room 2",
                    Capacity = 20,
                    Type = "Meeting",
                    Location = new Location
                    {
                        Id = 2,
                        Name = "Building 2",
                        AvailabilityStart = DateTime.Now.AddHours(-2),
                        AvailabilityEnd = DateTime.Now.AddHours(-1),
                    }
                },
                new Room{
                    Id = 3,
                    Name = "Room 3",
                    Capacity = 30,
                    Type = "Presentation",
                    Location = new Location
                    {
                        Id = 3,
                        Name = "Building 3",
                        AvailabilityStart = DateTime.Now.AddHours(-3),
                        AvailabilityEnd = DateTime.Now.AddHours(-2),
                    }
                }
            };

            return View(rooms);
    }
}