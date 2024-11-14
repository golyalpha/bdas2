using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

public class RequestController : Controller
{
    private readonly ILogger<RequestController> _logger;

    public RequestController(ILogger<RequestController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var requests = new List<Request>
            {
                new Request{
                    Id = 1, 
                    Start = DateTime.Now.AddHours(-1), 
                    End = DateTime.Now,
                    Organiser = new Organiser
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@doe.com"
                    }
                },
                new Request 
                {
                    Id = 2, 
                    Start = DateTime.Now.AddHours(-2), 
                    End = DateTime.Now.AddHours(-1),
                    Organiser = new Organiser
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@doe.com"
                    }
                },
                new Request 
                {
                    Id = 3, 
                    Start = DateTime.Now.AddHours(-3), 
                    End = DateTime.Now.AddHours(-2),
                    Organiser = new Organiser
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@doe.com"
                    }
                }
            };

            return View(requests);
    }
}