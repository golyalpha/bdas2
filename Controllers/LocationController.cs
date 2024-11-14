using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

public class LocationController : Controller
{
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILogger<LocationController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var locations = new List<Location>
            {
                new Location 
                {
                    Id = 1,
                    Name = "Building 1",
                    AvailabilityStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(-1)),
                    AvailabilityEnd = TimeOnly.FromDateTime(DateTime.Now),
                },
                new Location 
                {
                    Id = 2,
                    Name = "Building 2",
                    AvailabilityStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(-2)),
                    AvailabilityEnd = TimeOnly.FromDateTime(DateTime.Now.AddHours(-1)),
                },
                new Location 
                {
                    Id = 3,
                    Name = "Building 3",
                    AvailabilityStart = TimeOnly.FromDateTime(DateTime.Now.AddHours(-3)),
                    AvailabilityEnd = TimeOnly.FromDateTime(DateTime.Now.AddHours(-2)),
                }
            };

            return View(locations);
    }
}