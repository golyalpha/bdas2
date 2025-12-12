using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

[Authorize]
public class ReservationController : Controller
{
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(ILogger<ReservationController> logger)
    {
        _logger = logger;
    }

    // Index: Zobrazí seznam rezervací
    public IActionResult Index()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole("Administrator");
        
        var reservations = isAdmin 
            ? Reservation.ListReservations() 
            : Reservation.GetReservationsByOrganizerId(currentUserId);
            
        ViewBag.IsAdmin = isAdmin;
        ViewBag.IsManager = User.IsInRole("Manager");
        ViewBag.CurrentUserId = currentUserId;
        return View(reservations);
    }

    // GET: Reservation/Detail/{id}
    [HttpGet]
    public IActionResult Detail(int id)
    {
        try
        {
            var reservation = Reservation.GetReservation(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            var isManager = User.IsInRole("Manager");
            var isOwner = reservation.Organiser.Id == currentUserId;
            var isBuildingManager = isManager && reservation.Room?.Location?.Organiser?.Id == currentUserId;

            // Každý může vidět detail (admin, manager své budovy, owner své rezervace)
            // Pokud chcete omezit přístup, můžete přidat:
            // if (!isAdmin && !isOwner && !isBuildingManager)
            // {
            //     return Unauthorized();
            // }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsManager = isManager;
            ViewBag.IsOwner = isOwner;
            ViewBag.IsBuildingManager = isBuildingManager;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CanDelete = isAdmin || isOwner || isBuildingManager;

            return View(reservation);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při načítání detailu rezervace {ReservationId}", id);
            TempData["Error"] = "Chyba při načítání detailu rezervace";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            Reservation reservation = Reservation.GetReservation(id);
            
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            var isManager = User.IsInRole("Manager");
            
            // Admin může mazat vše
            if (isAdmin)
            {
                reservation.Delete();
                TempData["Success"] = "Rezervace byla úspěšně smazána";
                return RedirectToAction("Index");
            }
            
            // Manager může mazat rezervace v místnostech ve svých budovách
            if (isManager)
            {
                var roomLocation = reservation.Room.Location;
                if (roomLocation.Organiser.Id == currentUserId)
                {
                    reservation.Delete();
                    TempData["Success"] = "Rezervace byla úspěšně smazána";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Manager {UserId} attempted to delete reservation {ReservationId} in building owned by {BuildingOwnerId}", 
                        currentUserId, id, roomLocation.Organiser.Id);
                    TempData["Error"] = "Nemáte oprávnění smazat tuto rezervaci. Můžete mazat pouze rezervace ve vašich budovách.";
                    return RedirectToAction("Index");
                }
            }
            
            // Běžný uživatel může mazat pouze své rezervace
            if (reservation.Organiser.Id == currentUserId)
            {
                reservation.Delete();
                TempData["Success"] = "Rezervace byla úspěšně smazána";
                return RedirectToAction("Index");
            }
            
            _logger.LogWarning("User {UserId} attempted to delete reservation {ReservationId} owned by {OwnerId}", 
                currentUserId, id, reservation.Organiser.Id);
            TempData["Error"] = "Nemáte oprávnění smazat tuto rezervaci";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reservation.");
            TempData["Error"] = $"Chyba při mazání rezervace: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}
