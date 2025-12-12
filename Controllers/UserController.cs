using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Util;  

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
        ViewBag.IsGuest = User.IsInRole("Guest");  // PŘIDÁNO
        
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
    public async Task<IActionResult> Login(LoginViewModel request) 
    {
        if (!ModelState.IsValid) 
        {
            return View(request);
        }

        try
        {
            // Ověření hesla pomocí DB funkce
            int? userId = Credential.VerifyPassword(request.Email, request.Password);
            
            if (userId == null)
            {
                ModelState.AddModelError("", "Neplatný email nebo heslo");
                return View(request);
            }
            
            // Načtení uživatele
            var user = Organiser.GetOrganiser(userId.Value);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            
            _logger.LogInformation("Uživatel {Email} se úspěšně přihlásil", request.Email);
            return RedirectToAction("Index", "Request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při přihlášení");
            ModelState.AddModelError("", "Chyba při přihlášení");
            return View(request);
        }
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
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel request)
    {
        _logger.LogInformation("=== REGISTRACE START ===");
        _logger.LogInformation("Email: {Email}, Name: {Name}", request.Email, request.Name);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState není validní");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
            }
            return View(request);
        }

        try
        {
            // KROK 1: Kontrola existence emailu
            _logger.LogInformation("KROK 1: Kontrola existence emailu...");
            try
            {
                var existingUser = Organiser.FindOrganiser(request.Email);
                _logger.LogWarning("Email {Email} již existuje!", request.Email);
                ModelState.AddModelError("Email", "Email již existuje");
                return View(request);
            }
            catch (KeyNotFoundException) 
            { 
                _logger.LogInformation("Email {Email} je volný", request.Email);
            }

            // KROK 2: Vytvoření organizátora
            _logger.LogInformation("KROK 2: Vytváření organizátora...");
            var role = Role.ListRoles().FirstOrDefault(r => r.Name == "Guest");
            if (role == null)
            {
                _logger.LogError("Role 'Guest' nebyla nalezena!");
                throw new Exception("Role 'Guest' nebyla nalezena v databázi");
            }
            _logger.LogInformation("Role Guest nalezena: ID={RoleId}", role.Id);

            var organiser = new Organiser
            {
                Email = request.Email,
                Name = request.Name,
                Role = role
            };
            
            _logger.LogInformation("Volám organiser.Persist()...");
            organiser.Persist();
            _logger.LogInformation("Organiser.Persist() dokončen");
            
            // KROK 3: Načtení ID nově vytvořeného organizátora
            _logger.LogInformation("KROK 3: Načítání ID nově vytvořeného organizátora...");
            organiser = Organiser.FindOrganiser(request.Email);
            _logger.LogInformation("Načten organiser s ID: {OrganiserId}", organiser.Id);
            
            if (organiser.Id == null)
            {
                _logger.LogError("ID organizátora je NULL!");
                throw new Exception("Nepodařilo se získat ID nově vytvořeného uživatele");
            }

            // KROK 4: Hashování hesla
            _logger.LogInformation("KROK 4: Hashování hesla...");
            _logger.LogInformation("Heslo (plain): {Password}", request.Password);
            
            string hashedPassword;
            try
            {
                hashedPassword = Credential.HashPassword(request.Password);
                _logger.LogInformation("Hash hesla: {Hash} (délka: {Length})", 
                    hashedPassword, hashedPassword.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při hashování hesla!");
                throw new Exception($"Chyba při hashování hesla: {ex.Message}", ex);
            }

            // KROK 5: Uložení credentials
            _logger.LogInformation("KROK 5: Ukládání credentials...");
            var credential = new Credential
            {
                CredentialType = "PASSWORD",
                Data = hashedPassword,
                IdOrganizer = organiser.Id
            };
            
            _logger.LogInformation("Volám credential.Persist()...");
            credential.Persist();
            _logger.LogInformation("Credential.Persist() dokončen");

            _logger.LogInformation("=== REGISTRACE ÚSPĚŠNÁ ===");
            _logger.LogInformation("Email: {Email}, ID: {Id}", request.Email, organiser.Id);
            
            TempData["Success"] = "Registrace úspěšná! Můžete se přihlásit.";
            return RedirectToAction("Login");
        }
        catch (OracleException ex)
        {
            _logger.LogError(ex, "Oracle chyba při registraci");
            _logger.LogError("Oracle Error Number: {Number}", ex.Number);
            _logger.LogError("Oracle Error Message: {Message}", ex.Message);
            ModelState.AddModelError("", $"Chyba databáze: {ex.Message}");
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Obecná chyba při registraci");
            _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            ModelState.AddModelError("", $"Chyba: {ex.Message}");
            return View(request);
        }
    }


    [HttpGet]
    public IActionResult Details(int id)
    {
        var user = Organiser.GetOrganiser(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // POUZE ADMIN MŮŽE VIDĚT DETAILY JINÝCH UŽIVATELŮ
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        
        if (!isAdmin && currentUserId != id)
        {
            _logger.LogWarning("User {UserId} attempted to access details of user {TargetId}", currentUserId, id);
            return Forbid();
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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel 
        { 
            Email = string.Empty,
            NewPassword = string.Empty, 
            ConfirmPassword = string.Empty 
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Volání DB procedury pro reset hesla
            Credential.ResetPassword(model.Email, model.NewPassword);

            _logger.LogInformation("Heslo pro email {Email} bylo úspěšně resetováno", model.Email);

            TempData["Success"] = "Heslo bylo úspěšně změněno. Můžete se přihlásit.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při resetování hesla");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}