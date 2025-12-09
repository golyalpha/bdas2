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

    public IActionResult Index(string searchString)
    {
        var users = WebApp.Models.Organiser.ListOrganisers();

        if (!string.IsNullOrEmpty(searchString))
        {
            users = users.Where(u => u.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        ViewData["CurrentFilter"] = searchString;
        return View(users);
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string ReturnUrl)
    {
        // Pokud je uživatel již přihlášen, přesměruj na domovskou stránku
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Request");
        }
        
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
            
            // Přesměrování na domovskou stránku po úspěšném přihlášení
            return RedirectToAction("Index", "Request");
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

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult Impersonate()
    {
        var users = Organiser.GetNonAdminOrganisers();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ImpersonateUser(int userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;  // PŘIDÁNO: uložení původní role
        
        if (string.IsNullOrEmpty(currentUserId))
        {
            return RedirectToAction("Login");
        }

        // Načtení cílového uživatele
        Organiser targetUser;
        try
        {
            targetUser = Organiser.GetOrganiser(userId);
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Uživatel nebyl nalezen.";
            return RedirectToAction("Impersonate");
        }

        // Nelze impersonovat jiného administrátora
        if (targetUser.Role.Name == "Administrator")
        {
            TempData["Error"] = "Nelze impersonovat jiného administrátora.";
            return RedirectToAction("Impersonate");
        }

        // Vytvoření nových claims s impersonací
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()),
            new Claim(ClaimTypes.Name, targetUser.Name),
            new Claim(ClaimTypes.Role, targetUser.Role.Name),
            new Claim("OriginalUserId", currentUserId),
            new Claim("OriginalUserName", currentUserName),
            new Claim("OriginalUserRole", currentUserRole),  
            new Claim("IsImpersonating", "true")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        _logger.LogWarning("Administrátor {AdminName} (ID: {AdminId}) zahájil impersonaci uživatele {UserName} (ID: {UserId})", 
            currentUserName, currentUserId, targetUser.Name, targetUser.Id);

        TempData["Success"] = $"Nyní jednáte jako uživatel {targetUser.Name}.";
        return RedirectToAction("Index", "Request");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> StopImpersonation()
    {
        var isImpersonating = User.FindFirst("IsImpersonating")?.Value;
        
        if (isImpersonating != "true")
        {
            return RedirectToAction("Index", "Home");
        }

        var originalUserId = User.FindFirst("OriginalUserId")?.Value;
        var originalUserName = User.FindFirst("OriginalUserName")?.Value;
        var impersonatedUserName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(originalUserId))
        {
            return RedirectToAction("Login");
        }

        // Načtení původního administrátora
        Organiser originalUser;
        try
        {
            originalUser = Organiser.GetOrganiser(int.Parse(originalUserId));
        }
        catch (KeyNotFoundException)
        {
            return RedirectToAction("Login");
        }

        // Obnovení původních claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, originalUser.Id.ToString()),
            new Claim(ClaimTypes.Name, originalUser.Name),
            new Claim(ClaimTypes.Role, originalUser.Role.Name)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        _logger.LogWarning("Administrátor {AdminName} (ID: {AdminId}) ukončil impersonaci uživatele {UserName}", 
            originalUserName, originalUserId, impersonatedUserName);

        TempData["Success"] = "Impersonace byla ukončena. Jste přihlášeni jako vlastní účet.";
        return RedirectToAction("Index", "Request");
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult EditRole(int id)
    {
        var user = Organiser.GetOrganiser(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Roles = Role.ListRoles();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public IActionResult EditRole(int id, int roleId)
    {
        var user = Organiser.GetOrganiser(id);
        if (user == null)
        {
            return NotFound();
        }

        var newRole = Role.GetRole(roleId);
        user.Role = newRole;
        user.Persist();

        _logger.LogInformation("Administrátor změnil roli uživatele {UserName} (ID: {UserId}) na {NewRole}", 
            user.Name, user.Id, newRole.Name);

        TempData["Success"] = $"Role uživatele {user.Name} byla změněna na {newRole.Name}.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult EditSubstitute(int id)
    {
        var user = Organiser.GetOrganiser(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditSubstituteViewModel
        {
            User = user,
            CurrentSubstituteId = user.Substitute?.Id,
            PotentialSubstitutes = Organiser.GetPotentialSubstitutes(id)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public IActionResult EditSubstitute(int id, int? substituteId)
    {
        try
        {
            var user = Organiser.GetOrganiser(id);
            if (user == null)
            {
                return NotFound();
            }

            // Validace: nelze být sám sobě náhradníkem
            if (substituteId == id)
            {
                TempData["Error"] = "Uživatel nemůže být sám sobě náhradníkem.";
                return RedirectToAction("EditSubstitute", new { id });
            }

            user.UpdateSubstitute(substituteId);

            var substituteName = substituteId.HasValue
                ? Organiser.GetOrganiser(substituteId.Value).Name
                : "žádný";

            _logger.LogInformation("Administrátor změnil náhradníka uživatele {UserName} (ID: {UserId}) na {SubstituteName}",
                user.Name, user.Id, substituteName);

            TempData["Success"] = substituteId.HasValue
                ? $"Náhradník uživatele {user.Name} byl nastaven na {substituteName}."
                : $"Náhradník uživatele {user.Name} byl odebrán.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při změně náhradníka");
            TempData["Error"] = "Chyba při ukládání náhradníka: " + ex.Message;
            return RedirectToAction("EditSubstitute", new { id });
        }
    }
}
