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
                    AvailabilityStart = DateTime.Now.AddHours(-1),
                    AvailabilityEnd = DateTime.Now,
                },
                new Location 
                {
                    Id = 2,
                    Name = "Building 2",
                    AvailabilityStart = DateTime.Now.AddHours(-2),
                    AvailabilityEnd = DateTime.Now.AddHours(-1),
                },
                new Location 
                {
                    Id = 3,
                    Name = "Building 3",
                    AvailabilityStart = DateTime.Now.AddHours(-3),
                    AvailabilityEnd = DateTime.Now.AddHours(-2),
                }
            };

            return View(locations);
    }
}