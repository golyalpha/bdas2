using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
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
    [AllowAnonymous]
    public IActionResult Login(string ReturnUrl)
    {
        System.Console.WriteLine(ReturnUrl);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel request) {
        if (ModelState.IsValid) {
            Organiser user;
            try
            {
                user = Organiser.FindOrganiser(request.Email);
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("Email", "Invalid User");
                return View(request);
            }

            Credential cred = Credential.ListCredentials().Where((c) => c.IdOrganizer == user.Id).Last();

            if (cred.Data != request.Password)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View(request);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            return RedirectToAction("Index");
        }
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }


    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Register(RegisterViewModel request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var user = Organiser.FindOrganiser(request.Email);
            ModelState.AddModelError("Email", "Email already taken");
            return View(request);
        }
        catch (KeyNotFoundException) { }

        var role = Role.ListRoles().Where(r => r.Name == "Guest").First();
        var organiser = new Organiser
        {
            Email = request.Email,
            Name = request.Name,
            Role = role
        };
        organiser.Persist();
        organiser = Organiser.FindOrganiser(request.Email);  // Gotta grab the ID from DB
        if (organiser is null || organiser.Id is 0) {
            throw new InvalidOperationException(); // This should absolutely never have a chance of happening
        }
        var credential = new Credential
        {
            CredentialType = "PASSWORD",
            Data = request.Password,
            IdOrganizer = (int)organiser.Id
        };
        credential.Persist();
        return RedirectToAction("Login");
    }


    [HttpGet]
    public IActionResult Details(int id)
    {
        var user = Organiser.GetOrganiser(id);
        if (user == null)
        {
            return NotFound();
        }
        var reservations = Reservation.GetReservationsByOrganizerId(id);
        var requests = RoomRequest.GetRequestsByOrganiserId(id);
        var model = new UserDetailsViewModel
        {
            User = user,
            Reservations = reservations,
            Requests = requests
        };

        return View(model);
    }
}
