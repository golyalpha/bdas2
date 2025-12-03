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
        ViewBag.CurrentUserId = currentUserId;
        return View(reservations);
    }

    // GET: Room/Edit/{id}
    [HttpGet]
    [Route("Reservation/Edit/{id?}")]
    [Route("Reservation/Create")]
    public IActionResult Persist(int? id)
    {
        if (id == null || id == 0)
        {
            var newReservation = new Reservation
            {
                Id = 0,
                Start = new DateTime(),
                End = new DateTime(),
                Room = new Room { Id = 0 },
                Organiser = new Organiser { Id = 0 },
            };
            return View("Persist", newReservation); // Načte formulář Persist.cshtml
        }
        else
        {
            // Načítáme existující záznam
            var reservation = Reservation.GetReservation(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }
            
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            
            if (!isAdmin && reservation.Organiser.Id != currentUserId)
            {
                return Unauthorized();
            }
            
            return View("Persist", reservation); // Načte formulář Persist.cshtml
        }
    }

    // POST: Reservation/Persist
    [HttpPost]
    public IActionResult Persist(Reservation reservation)
    {
        if (reservation.Id != 0)
        {
            var existing = Reservation.GetReservation(reservation.Id);
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            
            if (!isAdmin && existing.Organiser.Id != currentUserId)
            {
                return Unauthorized();
            }
        }
        
        reservation.Persist();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            Reservation reservation = Reservation.GetReservation(id);
            
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Administrator");
            
            if (!isAdmin && reservation.Organiser.Id != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete reservation {ReservationId} owned by {OwnerId}", 
                    currentUserId, id, reservation.Organiser.Id);
                return Unauthorized();
            }
            
            reservation.Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reservation.");
        }
        return RedirectToAction("Index");
    }
}
