using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var users = WebApp.Models.Organiser.ListOrganisers();
        return View(users);
    }

    [HttpGet]
    public IActionResult Login() {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel request) {
        if (ModelState.IsValid) {
            return RedirectToAction("Index");
        }
        return View(request);
    }

    public IActionResult Register()
    {
        return View();
    }

}
